using AutoMapper;
using DShop2024.Areas.Admin.Models.Order;
using DShop2024.Areas.Admin.Models.Refund;
using DShop2024.EnumData;
using DShop2024.Models;
using DShop2024.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DShop2024.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Employee)]
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
        public async Task<IActionResult> Index(string searchOrderCode = "", [FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            ViewBag.sidebar = Menu.Admin.Order;

            var countCancelOrder = _context.Orders.Where(p => p.Status == 0).Count();
            var countNewOrder = _context.Orders.Where(p => p.Status == 1).Count();
            var countAcceptedOrder = _context.Orders.Where(p => p.Status == 2).Count();
            var countDeliveryOrder = _context.Orders.Where(p => p.Status == 3).Count();
            var countCompletedOrder = _context.Orders.Where(p => p.Status == 4).Count();

            ViewBag.countCancelOrder = countCancelOrder;
            ViewBag.countNewOrder = countNewOrder;
            ViewBag.countAcceptedOrder = countAcceptedOrder;
            ViewBag.countDeliveryOrder = countDeliveryOrder;
            ViewBag.countCompletedOrder = countCompletedOrder;


            IQueryable<OrderModel> listOrder = _context.Orders.OrderByDescending(o => o.CreatedDate);
            var count = await listOrder.CountAsync();
            if (count > 0)
            {
                if (!String.IsNullOrEmpty(searchOrderCode))
                {
                    listOrder = listOrder.Where(c => c.OrderCode == searchOrderCode);
                }
            }
            ViewBag.searchOrderCode = searchOrderCode;
            int totalOrder = listOrder.Count();
            ViewBag.totalOrder = totalOrder;
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
                    pagesSize = pagesSize,
                    searchOrderCode = searchOrderCode
                })
            };

            var orders = await listOrder.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .Include(u => u.User)
                        .Include(p => p.PaymentMethod)
                        .Include(p => p.Returns)
                        .ToListAsync();
            var orderVMs = _mapper.Map<List<OrderViewModel>>(orders);
            ViewBag.pagingModel = pagingModel;
            return View(orderVMs);
        }

        public async Task<IActionResult> ViewOrder(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .Include(c => c.UpdateBy)
                                                  .Include(c => c.PaymentMethod)
                                                  .Include(c => c.Returns)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);
            if (order == null)
            {
                return NotFound();
            }
            var orderVM = _mapper.Map<OrderViewModel>(order);
            if(orderVM.IsReturn)
            {
                if (!orderVM.IsFullReturn)
                {
                    return RedirectToAction("ViewOrderAndPartReturn", new { Id = order.Id });
                }
            }
            return View(orderVM);
        }


        public async Task<IActionResult> ViewOrderAndPartReturn(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders.Include(o => o.User)
                                                  .Include(od => od.OrderDetails)
                                                  .ThenInclude(p => p.Product)
                                                  .Include(c => c.OrderCoupons)
                                                  .ThenInclude(c => c.Coupon)
                                                  .Include(c => c.UpdateBy)
                                                  .Include(c => c.PaymentMethod)
                                                  .Include(c => c.Returns)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);

            var returnModel = await _context.Returns.Include(r => r.ReturnDetails).FirstOrDefaultAsync(r => r.OrderId == order.Id);
            if (order == null || returnModel == null)
            {
                return NotFound();
            }
            List<OrderAndReturnDetailVM> details = new List<OrderAndReturnDetailVM>();
            foreach (var item in order.OrderDetails)
            {
                OrderAndReturnDetailVM orderAndReturnDetail = new OrderAndReturnDetailVM
                {
                    ReturnDetailId = item.Id,
                    Price = item.Price,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    ProductImage = item.Product.MainImage,
                    OriginalQuantity = item.Quantity,
                    UnitPrice = item.Price * item.Quantity
                };
                var returnDetail = await _context.ReturnDetails.FirstOrDefaultAsync(rd => rd.OrderDetailId == item.Id);
                if(returnDetail != null)
                {
                    orderAndReturnDetail.ReturnedQuantity = returnDetail.Quantity;
                    orderAndReturnDetail.NetQtyKept = item.Quantity - returnDetail.Quantity;
                    orderAndReturnDetail.Status = GetStatusDetailReturn(item.Quantity, returnDetail.Quantity);
                }
                else
                {
                    orderAndReturnDetail.ReturnedQuantity = 0;
                    orderAndReturnDetail.NetQtyKept = item.Quantity;
                    orderAndReturnDetail.Status = OrderEnumData.KEPT;
                }
                details.Add(orderAndReturnDetail);
            }

            OrderWithOrderAndReturnDetailsVM model = new OrderWithOrderAndReturnDetailsVM { 
                Order = order,
                Return = returnModel,
                Details = details,
                GrandTotal = order.GrandTotal,
                RefundAmount = returnModel.TotalRefundAmount,
                RemainingAmount = order.GrandTotal - returnModel.TotalRefundAmount
            };
            return View(model);
        }

        private string GetStatusDetailReturn (int originalQty, int returnQuantity)
        {
            if(originalQty > returnQuantity)
            {
                return OrderEnumData.PARTIALLY_RETURNED;
            }
            if (originalQty == returnQuantity)
            {
                return OrderEnumData.FULLY_RETURNED;
            }
            return OrderEnumData.KEPT;
        }


        public async Task<IActionResult> UpdateStatusOrder(int? orderId)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            try
            {
                var user = await _userManager.GetUserAsync(this.User);
                if (order.Status != 4)
                {
                    order.Status += 1;
                }
                if(order.Status == 4)
                {
                    order.PaymentStatus = 1;
                }
                order.DateUpdate = DateTime.Now;
                order.UserIdUpdate = user.Id;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Update Status order successful";
                return RedirectToAction("ViewOrder", "Order", new { order.Id });

            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Update status order fail " + ex.Message;
                return RedirectToAction("ViewOrder", "Order", new { order.Id });
            }


        }

        public async Task<IActionResult> CancelOrder(int? orderId)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (orderId == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FirstOrDefaultAsync(od => od.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status == 0)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Deleted order";
                return RedirectToAction("Index");
            }
            if (order.Status == 4)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cannot cancel completed order ";
                return RedirectToAction("Index");
            }
            try
            {

                var listOrderDetail = await _context.OrderDetails.Where(d => d.OrderId == orderId).ToListAsync();
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
                TempData[DShopConst.TEMPDATA_SUCCESS] = "Cancle order successful";
                return RedirectToAction("ViewOrder", "Order", new { order.Id });

            }
            catch (Exception ex)
            {
                TempData[DShopConst.TEMPDATA_ERROR] = "Cancle order fail " + ex.Message;
                return RedirectToAction("ViewOrder", "Order", new { order.Id });
            }


        }

        [Authorize(Roles = RoleName.Administrator)]
        public async Task<IActionResult> ManagePaymentMethod()
        {
            ViewBag.sidebar = Menu.Admin.Order;
            var payments = await _context.Payments.ToListAsync();
            return View(payments);

        }

        [HttpPost]
        public async Task<IActionResult> SetStatusPaymentMethod(int idPayment, int status)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            try
            {
                var payment = await _context.Payments.FindAsync(idPayment);
                payment.Status = status;
                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, Message = "Set active payment successful" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, Message = ex.Message });
            }

        }

        public async Task<IActionResult> ManageReturn(string search = "",[FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            var listReturn = _context.Returns.Include(r => r.Customer)
                                            .Include(r => r.Order)
                                            .OrderByDescending(r => r.ReturnDate)
                                            .AsQueryable();
            int totaReturns = listReturn.Count();
            if (totaReturns > 0)
            {
                if (!String.IsNullOrEmpty(search))
                {
                    listReturn = listReturn.Where(c => c.Order.OrderCode == search || c.Reason.Contains(search) 
                    || c.Customer.UserName.Contains(search) || c.Description.Contains(search) );
                }
            }
            ViewBag.search= search;
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totaReturns / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("ManageReturn", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var returns = await listReturn.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize)
                        .ToListAsync();
            var returnVMs = _mapper.Map<List<ReturnViewModel>>(returns);
            ViewBag.pagingModel = pagingModel;
            return View(returnVMs);
        }

        public async Task<IActionResult> DetailFullReturn(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var returnModel = await _context.Returns.Include(o => o.Customer)
                                             .Include(od => od.UpdateBy)
                                             .Include(od => od.ReturnDetails)
                                             .ThenInclude(p => p.Product)
                                             .Include(c => c.Order)
                                             .FirstOrDefaultAsync(o => o.Id == Id);
            if (returnModel == null)
            {
                return NotFound();
            }
            var returnVMs = _mapper.Map<ReturnViewModel>(returnModel);
            return View(returnVMs);

        }
        public async Task<IActionResult> DetailPartReturn(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var returnModel = await _context.Returns.Include(o => o.Customer)
                                             .Include(od => od.UpdateBy)
                                             .Include(od => od.ReturnDetails)
                                             .ThenInclude(p => p.Product)
                                             .Include(c => c.Order)
                                             .FirstOrDefaultAsync(o => o.Id == Id);
            if (returnModel == null)
            {
                return NotFound();
            }

            var returnVMs = _mapper.Map<ReturnViewModel>(returnModel);
            return View(returnVMs);
        }

        public async Task<IActionResult> AcceptedReturnStatus(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var returnModel = await _context.Returns.Include(o => o.Order)
                                             .Include(o => o.ReturnDetails)
                                             .FirstOrDefaultAsync(o => o.Id == Id);
            if (returnModel == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);

            returnModel.Status = 2;
            returnModel.UpdateDate = DateTime.Now;
            returnModel.UpdateUserId = user.Id;
            _context.Returns.Update(returnModel);
            await _context.SaveChangesAsync();

            RefundModel refundModel = new RefundModel
            {
                Amount = returnModel.TotalRefundAmount,
                OrderCode = returnModel.Order.OrderCode,
                Reason = OrderEnumData.REASON_REFUND_RETURN,
                CreateDate = DateTime.Now,
                Status = ((int)OrderEnumData.StatusRefund.Approved)
            };
            await _context.Refunds.AddAsync(refundModel);
            await _context.SaveChangesAsync();
            await ImportoOfReturnedProducts(returnModel);
            if (String.IsNullOrEmpty(returnModel.Reason))
            {
                return RedirectToAction("DetailPartReturn", new { id = returnModel.Id});
            }
            return RedirectToAction("DetailFullReturn", new { id = returnModel.Id });
        }

        private async Task ImportoOfReturnedProducts(ReturnModel returnGood)
        {
            foreach (var item in returnGood.ReturnDetails)
            {
                ProductModel product = await _context.Products.Where(s => s.Status != 0).FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if(product != null)
                {
                    product.Stock += item.Quantity;
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<IActionResult> RejectReturnStatus(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var returnModel = await _context.Returns
                                             .FirstOrDefaultAsync(o => o.Id == Id);
            if (returnModel == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(this.User);
            returnModel.Status = 0;
            returnModel.UpdateDate = DateTime.Now;
            returnModel.UpdateUserId = user.Id;
            _context.Returns.Update(returnModel);
            await _context.SaveChangesAsync();
            if (String.IsNullOrEmpty(returnModel.Reason))
            {
                return RedirectToAction("DetailPartReturn", new { id = returnModel.Id });
            }
            return RedirectToAction("DetailFullReturn", new { id = returnModel.Id });
        }

        public async Task<IActionResult> ManageRefund(string search ="",[FromQuery(Name = "p")] int currentPage = 1, int pagesSize = 10)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            IQueryable<RefundModel> listRefund = _context.Refunds.Where(c => c.Status != 0)
                                                        .OrderByDescending(d => d.CreateDate).AsQueryable();
            int totalRefund = listRefund.Count();
            if (totalRefund > 0)
            {
                if (!String.IsNullOrEmpty(search))
                {
                    listRefund = listRefund.Where(c => c.OrderCode == search || c.TransactionId == search 
                    || c.TransactionContent.Contains(search) || c.Reason.Contains(search) );
                }
            }
            ViewBag.search = search;
            if (pagesSize <= 0)
                pagesSize = 10;
            int countPages = (int)Math.Ceiling((double)totalRefund / 10);

            if (currentPage > countPages)
                currentPage = countPages;
            if (currentPage < 1)
                currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("ManageRefund", new
                {
                    p = pageNumber,
                    pagesSize = pagesSize
                })
            };

            var refunds = await listRefund.Skip((currentPage - 1) * pagesSize)
                        .Take(pagesSize).ToListAsync();

            ViewBag.pagingModel = pagingModel;
            return View(refunds);
        }

        [HttpGet]
        public async Task<IActionResult> GetPopUpRefundReturn(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var returnModel = await _context.Returns.Include(r => r.Order).Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status == 2);
            if (returnModel == null)
            {
                return NotFound();
            }
            var refund = await _context.Refunds.Where(r => r.OrderCode == returnModel.Order.OrderCode).FirstOrDefaultAsync();
            var updateRefund = _mapper.Map<UpdateRefundRequest>(refund);
            ViewBag.Payments = new SelectList(await _context.Payments.Where(s => s.Status != 0 && s.PaymentName != PaymentEnumData.COD).ToListAsync(), "Id", "PaymentName");
            return PartialView("_RefundReturnPartial", updateRefund);
        }

        [HttpPost]
        public async Task<IActionResult> RefundReturn(RefundModel refund)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (refund == null)
            {
                return NotFound();
            }
            var orderModel = await _context.Orders
                                  .FirstOrDefaultAsync(o => o.OrderCode == refund.OrderCode);

            var returnModel = await _context.Returns
                                             .FirstOrDefaultAsync(o => o.OrderId == orderModel.Id);
            if (returnModel == null || orderModel == null)
            {
                return NotFound();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(this.User);
                    returnModel.UpdateDate = DateTime.Now;
                    returnModel.UpdateUserId = user.Id;
                    returnModel.Status = 3;
                    _context.Returns.Update(returnModel);
                    await _context.SaveChangesAsync();
                    if (refund.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/refund");
                        string imageName = Guid.NewGuid().ToString() + "_" + refund.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await refund.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        refund.Image = "refund/" + imageName;
                    }
                    refund.RefundDate = DateTime.Now;
                    refund.Status = 2;
                    _context.Refunds.Update(refund);
                    orderModel.PaymentStatus = 2;
                    _context.Orders.Update(orderModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Refund return successful";
                    if (String.IsNullOrEmpty(returnModel.Reason))
                    {
                        return RedirectToAction("DetailPartReturn", new { id = returnModel.Id });
                    }
                    return RedirectToAction("DetailFullReturn", new { id = returnModel.Id });
                }
                TempData[DShopConst.TEMPDATA_ERROR] = "Please fill all infomations ";
                if (String.IsNullOrEmpty(returnModel.Reason))
                {
                    return RedirectToAction("DetailPartReturn", new { id = returnModel.Id });
                }
                return RedirectToAction("DetailFullReturn", new { id = returnModel.Id });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.Message.Contains("duplicate"))
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Duplicate transasionID " + ex.InnerException.Message;
                }
                else
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Refund return fail " + ex.Message;
                }                 
                if (String.IsNullOrEmpty(returnModel.Reason))
                {
                    return RedirectToAction("DetailPartReturn", new { id = returnModel.Id });
                }
                return RedirectToAction("DetailFullReturn", new { id = returnModel.Id });
            }
           
        }

        public async Task<IActionResult> ViewRefund(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var refund = await _context.Refunds.Include(o => o.Payment)
                                                  .FirstOrDefaultAsync(o => o.Id == Id);
            if (refund == null)
            {
                return NotFound();
            }
            return View(refund);
        }

        [HttpGet]
        public async Task<IActionResult> GetPopUpRefund(int? Id)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            if (Id == null)
            {
                return NotFound();
            }
            var refundModel = await _context.Refunds.Include(r => r.Payment)
                .FirstOrDefaultAsync(m => m.Id == Id && m.Status == 1);
            if (refundModel == null)
            {
                return NotFound();
            }
            ViewBag.Payments = new SelectList(await _context.Payments.Where(s => s.Status != 0 && s.PaymentName != PaymentEnumData.COD).ToListAsync(), "Id", "PaymentName");
            var updateRefund = _mapper.Map<UpdateRefundRequest>(refundModel);
            return PartialView("_RefundPartial", updateRefund);
        }

        [HttpPost]
        public async Task<IActionResult> RefundOder(RefundModel refund)
        {
            ViewBag.sidebar = Menu.Admin.Order;
            var orderModel = await _context.Orders.Include(r => r.Returns)
                      .FirstOrDefaultAsync(o => o.OrderCode == refund.OrderCode);
            if (orderModel == null)
            {
                return NotFound();
            }
            var returnModel = orderModel.Returns.FirstOrDefault();

            try
            {
                if (ModelState.IsValid)
                {
                    if(returnModel != null)
                    {
                        var user = await _userManager.GetUserAsync(this.User);
                        returnModel.UpdateDate = DateTime.Now;
                        returnModel.UpdateUserId = user.Id;
                        returnModel.Status = 3;
                        _context.Returns.Update(returnModel);
                        await _context.SaveChangesAsync();
                    }

                    if (refund.ImageUpload != null)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/refund");
                        string imageName = Guid.NewGuid().ToString() + "_" + refund.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadsDir, imageName);

                        FileStream fs = new FileStream(filePath, FileMode.Create);
                        await refund.ImageUpload.CopyToAsync(fs);
                        fs.Close();
                        refund.Image = "refund/" + imageName;
                    }
                    refund.RefundDate = DateTime.Now;
                    refund.Status = 2;
                    _context.Refunds.Update(refund);
                    orderModel.PaymentStatus = 2;
                    _context.Orders.Update(orderModel);
                    await _context.SaveChangesAsync();
                    TempData[DShopConst.TEMPDATA_SUCCESS] = "Refund return successful";
                    return RedirectToAction("ViewRefund", new { id = refund.Id });
                }
                TempData[DShopConst.TEMPDATA_ERROR] = "Please fill all infomations " ;
                return RedirectToAction("ViewRefund", new { id = refund.Id });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.Message.Contains("duplicate"))
                {
                    TempData[DShopConst.TEMPDATA_ERROR] = "Duplicate transasionID " + ex.InnerException.Message;
                    return RedirectToAction("ViewRefund", new { id = refund.Id });
                }
                TempData[DShopConst.TEMPDATA_ERROR] = "Refund return fail " + ex.Message;
                return RedirectToAction("ViewRefund", new { id = refund.Id });
            }

        }

    }
}
