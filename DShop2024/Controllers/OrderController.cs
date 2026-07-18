using AutoMapper;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Controllers
{
    [Authorize(Roles = RoleName.Customer)]
    public class OrderController : Controller
    {
        private readonly DShopContext _context;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public OrderController(DShopContext context, UserManager<AppUserModel> userManager, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            var user = await _userManager.GetUserAsync(this.User);
            IQueryable<OrderModel> listOrder = _context.Orders
                                                  .Where(o => o.UserId == user.Id)
                                                  .OrderByDescending(o => o.CreatedDate);

            int totalOrder = await listOrder.CountAsync();
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalOrder / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var orders = await listOrder.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Include(c => c.PaymentMethod)
                        .Include(c => c.Returns)
                        .ToListAsync();

            var orderVMs = _mapper.Map<List<OrderViewModel>>(orders);
            ViewBag.pagingModel = pagingModel;
            return View(orderVMs);
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);

            var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.Returns)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .Include(c => c.PaymentMethod)
                                                  .Where(o => o.UserId == user.Id)
                                                  .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var orderVM = _mapper.Map<OrderViewModel>(order);
            if(orderVM.IsReturn)
            {
                if (!orderVM.IsFullReturn)
                {
                    return RedirectToAction("ViewReturnPartOrder", new { id = order.Id });
                }
                return RedirectToAction("ViewReturnFullOrder", new { id = order.Id });

            }
            return View(orderVM);

        }

        public async Task<IActionResult> CancelOrder(int? id)
        {
            try
            {
                if(id == null)
                {
                    return NotFound();
                }
                var order = await _context.Orders.Include(o => o.PaymentMethod).Where(o => o.Status != 0).FirstOrDefaultAsync( o => o.Id == id );
                if( order == null)
                {
                    return NotFound();
                }
                if( order.Status > 1)
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "The order cannot be canceled. Because the order has been processed.";
                    return RedirectToAction("Index");
                }
                if(order.Status == 1)
                {
                    var listOrderDetail = await _context.OrderDetails.Where(d => d.OrderId == order.Id).ToListAsync();
                    if (listOrderDetail.Count > 0)
                    {
                        foreach (var item in listOrderDetail)
                        {
                            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
                            product.Stock += item.Quantity;
                            _context.Products.Update(product);
                            await _context.SaveChangesAsync();
                        }
                    }
                    var user = await _userManager.GetUserAsync(this.User);
                    order.Status = 0;
                    order.UserIdUpdate = user.Id;
                    order.DateUpdate = DateTime.Now;
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    if (order.PaymentStatus == 1)
                    {
                        RefundModel refundModel = new RefundModel
                        {
                            Amount = order.GrandTotal,
                            OrderCode = order.OrderCode,
                            Reason = OrderEnumData.REASON_REFUND_CANCEL,
                            CreateDate = DateTime.Now,
                            Status = (int) OrderEnumData.StatusRefund.Approved
                        };
                        await _context.Refunds.AddAsync(refundModel);
                        await _context.SaveChangesAsync();
                    }

                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Cancel order successful.";
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cancel order fail " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> GetPopUpReturn(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Where(o => o.Id == Id)
                                                .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != 4)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "The order cannot be return. Because the order has been processed.";
                return RedirectToAction("Index");
            }
            var orderVM = _mapper.Map<OrderViewModel>(order);
            orderVM.TotalQuantity = await _context.OrderDetails.Where(od => od.OrderId == order.Id).SumAsync(s => s.Quantity);
            return PartialView("_ReturnPopupPartial", orderVM);
        }

        public async Task<IActionResult> ReturnFullOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            var order = await _context.Orders.Where(o => o.Id == id)
                                              .Where(o => o.UserId == user.Id)
                                              .Include(o => o.User)
                                              .Include(o => o.Returns)
                                              .Include(od => od.OrderDetails)
                                              .ThenInclude(p => p.Product)
                                              .Include(c => c.PaymentMethod)
                                              .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != 4)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "The order cannot be return. Because the order has been processed.";
                return RedirectToAction("Index");
            }
            ViewBag.Reasons = new SelectList(ReasonReturnEnumData.Reasons);
            var orderVM= _mapper.Map<OrderViewModel>(order);
            return View(orderVM);
        }

        public async Task<IActionResult> ReturnPartOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            var order = await _context.Orders.Where(o => o.Id == id)
                                              .Where(o => o.UserId == user.Id)
                                              .Include(o => o.User)
                                              .Include(od => od.OrderDetails)
                                              .ThenInclude(p => p.Product)
                                              .Include(c => c.OrderCoupons)
                                              .ThenInclude(c => c.Coupon)
                                              .Include(c => c.PaymentMethod)
                                              .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }
            if (order.Status != 4)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "The order cannot be return. Because the order has been processed.";
                return RedirectToAction("Index");
            }

            var listReturnDetails = _mapper.Map<List<CreateReturnDetailRequest>>(order.OrderDetails);
            ViewBag.OrderCode = order.OrderCode;
            ViewBag.Reasons = new SelectList(ReasonReturnEnumData.Reasons);
            var checkReturn = await _context.Returns.AnyAsync(r => r.OrderId == order.Id);
            ViewBag.CheckReturn = checkReturn;
            return View(listReturnDetails);
        }

        public async Task<IActionResult> ViewReturnPartOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            var order = await _context.Orders.Where(o => o.Id == id && o.UserId == user.Id)
                                              .Include(o => o.User)
                                              .Include(od => od.OrderDetails)
                                              .ThenInclude(p => p.Product)
                                              .Include(c => c.OrderCoupons)
                                              .ThenInclude(c => c.Coupon)
                                              .Include(c => c.PaymentMethod)
                                              .FirstOrDefaultAsync();
            var returnModel = await _context.Returns.Include(r => r.ReturnDetails).ThenInclude(rt => rt.Product).Where(r => r.OrderId == order.Id).FirstOrDefaultAsync();

            if (order == null || returnModel == null)
            {
                return NotFound();
            }
            OrderAndReturnPartViewModel orderAndReturn = new OrderAndReturnPartViewModel
            {
                Order = _mapper.Map<OrderViewModel>(order),
                Return = _mapper.Map<ReturnViewModel>(returnModel)
            };
            return View(orderAndReturn);
        }

        public async Task<IActionResult> ViewReturnFullOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            var returnModel = await _context.Returns.Include(r => r.ReturnDetails)
                                                    .ThenInclude(rt => rt.Product)
                                                    .Include(rt => rt.Order)
                                                    .Include(rt => rt.Customer)
                                                    .Where(r => r.OrderId == id && r.UserId == user.Id)
                                                    .FirstOrDefaultAsync();

            if ( returnModel == null)
            {
                return NotFound();
            }
            var returnVM = _mapper.Map<ReturnViewModel>(returnModel);
            return View(returnVM);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFullReturn(ReturnModel returnModel)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == returnModel.OrderId);
            var user = await _userManager.GetUserAsync(this.User);
            if(order == null)
            {
                return NotFound();
            }
            if (order.UserId != user.Id)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "You are not allow to do this.";
                return RedirectToAction("ReturnFullOrder", new { id = order.Id });
            }
            if (String.IsNullOrEmpty(returnModel.Reason))
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Please select reason for return.";
                ModelState.AddModelError("", "This product already exists.");
                return RedirectToAction("ReturnFullOrder", new { id = order.Id });
            }

            try
            {
                List<string> images = new List<string>();
                if (returnModel.ImageFiles.Count > 0)
                {
                    string imgagePath = $"media/return/{user.UserName}";
                    var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, imgagePath);
                    if (!Directory.Exists(uploadDirectory))
                    {
                        Directory.CreateDirectory(uploadDirectory);
                    }
                    foreach (var file in returnModel.ImageFiles)
                    {
                        string imageReturnImages = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadDirectory, imageReturnImages);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        var itemListImage = $"{imgagePath}/{imageReturnImages}";
                        images.Add(itemListImage);
                    }
                }

                ReturnModel model = new ReturnModel
                {
                    Reason = returnModel.Reason,
                    Description = returnModel.Description,
                    ReturnDate = DateTime.Now,
                    Status = 1,
                    TotalRefundAmount = order.GrandTotal, //The shop is responsible for paying the shipping costs.
                    ReturnShippingCost = await GetShippingCost(order.AddressDelivery),
                    UserId = order.UserId,
                    OrderId = order.Id,
                    UpdateDate = DateTime.Now,
                    UpdateUserId = order.UserId,
                    Images = String.Join("|", images)
                };
                await _context.Returns.AddAsync(model);
                await _context.SaveChangesAsync();
                List<ReturnDetailModel> listReturnItems = new List<ReturnDetailModel>();

                foreach (var returnItem in order.OrderDetails)
                {
                    ReturnDetailModel returnDetail = new ReturnDetailModel
                    {
                        ReturnId = model.Id,
                        Description = model.Description,
                        ReturnReason = model.Reason,
                        PricePerUnit = returnItem.Price,
                        OriginalPricePerUnit = returnItem.OriginalPrice,
                        Quantity = returnItem.Quantity,
                        ProductId = returnItem.ProductId,
                        OrderDetailId = returnItem.Id
                    };
                    listReturnItems.Add(returnDetail);
                }
                await _context.ReturnDetails.AddRangeAsync(listReturnItems);
                await _context.SaveChangesAsync();

                TempData[DShopConst.TEMPDATA_SUCCESS] = "Full return successful";
                return RedirectToAction("ViewReturnFullOrder", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Full return fail " + ex.Message;
                return RedirectToAction("ReturnFullOrder", new { id = order.Id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePartReturn(string orderCode,List<CreateReturnDetailRequest> returnDetails)
        {
            var user = await _userManager.GetUserAsync(this.User);
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.Status == 4 && o.UserId == user.Id);
            if(order == null)
            {
                return NotFound();
            }

            var listReturn = returnDetails.Where(s => s.Status == true);
            if (listReturn.Count() == 0)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = $"Please select the product you want to return";
                return RedirectToAction("ReturnPartOrder", new { id = order.Id });
            }

            var qtyOrder = order.OrderDetails.Sum(od => od.Quantity);
            var qtyReturn  = returnDetails.Where(s => s.Status == true).Sum(od => od.Quantity);
            if(qtyReturn >= qtyOrder)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = $"All items are being returned. Please use the full order return option.";
                return RedirectToAction("ReturnFullOrder", new { id = order.Id });
            }


            try
            {

                decimal totalPrice = 0;
                foreach (var item in listReturn)
                {
                    var orderDetail = await _context.OrderDetails.Include(od => od.Product).FirstOrDefaultAsync(od => od.Id == item.OrderDetailId);
                    if(item.Quantity > orderDetail.Quantity)
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = $"The {orderDetail.Product.ProductName} product quantity exceeded. Maximum is {orderDetail.Quantity}";
                        return RedirectToAction("ReturnPartOrder", new { id = order.Id});
                    }
                    if (String.IsNullOrEmpty(item.ReturnReason))
                    {
                        TempData[DShopConst.TEMPDATA_ERROR] = $"Please select a return reason for the {orderDetail.Product.ProductName} product";
                        return RedirectToAction("ReturnPartOrder", new { id = order.Id });
                    }
                    totalPrice += item.Quantity * item.PricePerUnit;
                }

                totalPrice += order.ShippingCost;
                if (totalPrice > order.GrandTotal)
                {
                    totalPrice = order.GrandTotal; //If the refund amount is greater than the amount the customer paid, the refund amount = the amount the customer paid 
                }


                ReturnModel returnModel = new ReturnModel
                {
                    ReturnDate = DateTime.Now,
                    Status = 1,
                    TotalRefundAmount = totalPrice,
                    ReturnShippingCost = await GetShippingCost(order.AddressDelivery),
                    UserId = order.UserId,
                    OrderId = order.Id,
                    UpdateDate = DateTime.Now,
                    UpdateUserId = order.UserId
                };
                await _context.Returns.AddAsync(returnModel);
                await _context.SaveChangesAsync();
                List<ReturnDetailModel> listReturnItems = new List<ReturnDetailModel>();

                foreach (var returnItem in listReturn)
                {

                    List<string> images = new List<string>();
                    if (returnItem.ImageFiles.Count > 0)
                    {
                        string imgagePath = $"media/return/{user.UserName}";
                        var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, imgagePath);
                        if (!Directory.Exists(uploadDirectory))
                        {
                            Directory.CreateDirectory(uploadDirectory);
                        }
                        foreach (var file in returnItem.ImageFiles)
                        {
                            string imageReturnImages = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var filePath = Path.Combine(uploadDirectory, imageReturnImages);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            var itemListImage = $"{imgagePath}/{imageReturnImages}";
                            images.Add(itemListImage);
                        }
                    }
                    ReturnDetailModel returnDetail = new ReturnDetailModel
                    {
                        ReturnId = returnModel.Id,
                        Description = returnItem.Description,
                        ReturnReason = returnItem.ReturnReason,
                        PricePerUnit = returnItem.PricePerUnit,
                        OriginalPricePerUnit = returnItem.OriginalPricePerUnit,
                        Quantity = returnItem.Quantity,
                        ProductId = returnItem.ProductId,
                        OrderDetailId = returnItem.OrderDetailId,
                        Images = String.Join("|", images)
                    };
                    listReturnItems.Add(returnDetail);
                }
                await _context.ReturnDetails.AddRangeAsync(listReturnItems);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Part return successful";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Part return fail " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        private async Task<decimal> GetShippingCost(string addressDelivery)
        {
            if (string.IsNullOrWhiteSpace(addressDelivery))
                return DShopConst.DEFAULT_SHIPPING_COST;

            var province = addressDelivery.Split('_').Last().Trim();

            var shippingCost = await _context.Shippings
                .Where(s => s.Province == province)
                .Select(s => (decimal?)s.Price)
                .FirstOrDefaultAsync();

            return shippingCost ?? DShopConst.DEFAULT_SHIPPING_COST;
        }



    }
}
