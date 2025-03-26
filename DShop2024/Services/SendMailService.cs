using System;
using System.IO;
using System.Threading.Tasks;
using DShop2024.EnumData;
using DShop2024.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

public class MailSettings
{
    public string Mail { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }

}

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendSmsAsync(string number, string message);
    Task SendEmailOrder(OrderModel order, InformationShopModel infoShop);
	Task SendEmailContact(ContactModel contact, InformationShopModel infoShop);
}

public class SendMailService : IEmailSender
{


    private readonly MailSettings mailSettings;

    private readonly ILogger<SendMailService> logger;

    private readonly IWebHostEnvironment _webHostEnvironment;



    // mailSetting được Inject qua dịch vụ hệ thống
    // Có inject Logger để xuất log
    public SendMailService(IOptions<MailSettings> _mailSettings, ILogger<SendMailService> _logger, IWebHostEnvironment webHostEnvironment)
    {
        mailSettings = _mailSettings.Value;
        _webHostEnvironment = webHostEnvironment;
        logger = _logger;
        logger.LogInformation("Create SendMailService");
    }


    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();
        message.Sender = new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail);
        message.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;


        var builder = new BodyBuilder();
        builder.HtmlBody = htmlMessage;
        message.Body = builder.ToMessageBody();

        // dùng SmtpClient của MailKit
        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        try
        {
            //smtp.Connect (mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            //smtp.Authenticate (mailSettings.Mail, mailSettings.Password);
            await smtp.ConnectAsync(mailSettings.Host, mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(mailSettings.Mail, "rzskdmicavdwmjyo");
            await smtp.SendAsync(message);
        }

        catch (Exception ex)
        {
            // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
            System.IO.Directory.CreateDirectory("mailssave");
            var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
            await message.WriteToAsync(emailsavefile);

            logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailsavefile);
            logger.LogError(ex.Message);
        }

        smtp.Disconnect(true);

        logger.LogInformation("send mail to " + email);


    }

    public Task SendSmsAsync(string number, string message)
    {
        // Cài đặt dịch vụ gửi SMS tại đây
        System.IO.Directory.CreateDirectory("smssave");
        var emailsavefile = string.Format(@"smssave/{0}-{1}.txt", number, Guid.NewGuid());
        System.IO.File.WriteAllTextAsync(emailsavefile, message);
        return Task.FromResult(0);
    }


    public async Task SendEmailOrder(OrderModel order, InformationShopModel infoShop)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;

        var strProduct = "";
        foreach (var item in order.OrderDetails)
        {
            strProduct += "<tr>";
            strProduct += "<td>" + item.Product.ProductName + "</td>";
            strProduct += "<td>" + item.Quantity + "</td>";
            strProduct += "<td>" + (item.Price * item.Quantity).ToString("#,##0 VND") + "</td>";
            strProduct += "</tr>";

        }
        string path = "";
        path = System.IO.File.ReadAllText(Path.Combine(webRootPath, "media\\Email\\sendOrder.html"));
        path = path.Replace("{{OrderCode}}", order.OrderCode);
        path = path.Replace("{{Products}}", strProduct);
        path = path.Replace("{{CustomerName}}", order.Consignee);
        path = path.Replace("{{PaymentMetod}}", order.PaymentMethod);
        path = path.Replace("{{Phone}}", order.PhoneDelivery);
        path = path.Replace("{{Mail}}", order.User.Email);
        path = path.Replace("{{AddressDelivery}}", order.AddressDelivery);
        path = path.Replace("{{DateOrder}}", order.CreatedDate.ToShortDateString());
        path = path.Replace("{{PaymentMetod}}", order.PaymentMethod);
        path = path.Replace("{{Subtotal}}", order.OrderDetails.Sum(od => od.Quantity * od.Price).ToString("#,##0 VND"));
        path = path.Replace("{{ShippingCost}}", order.ShippingCost.ToString("#,##0 VND"));
        path = path.Replace("{{CouponValue}}", order.ValueCoupon.ToString("#,##0 VND"));
        path = path.Replace("{{GrandTotal}}", order.TotalPrice.ToString("#,##0 VND"));

        path = path.Replace("{{ShopName}}", infoShop.ShopName);
        path = path.Replace("{{EmailShop}}", infoShop.Email);
        path = path.Replace("{{HotlineShop}}", infoShop.Phone);


        await SendEmailAsync(order.User.Email, "Order DShop2024", path);
    }


	public async Task SendEmailContact(ContactModel contact, InformationShopModel infoShop)
	{
		string webRootPath = _webHostEnvironment.WebRootPath;

		string path = "";
		path = System.IO.File.ReadAllText(Path.Combine(webRootPath, "media\\Email\\sendContact.html"));
		path = path.Replace("{{Customer}}", contact.User.UserName);
		path = path.Replace("{{TimeSend}}", contact.DateSent.ToShortTimeString());
		path = path.Replace("{{DateSend}}", contact.DateSent.ToShortDateString());
		path = path.Replace("{{Subject}}", contact.Subject);
		path = path.Replace("{{Message}}", contact.Message);
		path = path.Replace("{{Respondent}}", contact.Respondent.UserName);
		path = path.Replace("{{TimeRespone}}", contact.DateRespone.ToShortTimeString());
		path = path.Replace("{{DateRespone}}", contact.DateRespone.ToShortDateString());
		path = path.Replace("{{ReplyMEssage}}", contact.ReplyMessage);


		path = path.Replace("{{ShopName}}", infoShop.ShopName);
		path = path.Replace("{{EmailShop}}", infoShop.Email);
		path = path.Replace("{{HotlineShop}}", infoShop.Phone);


		await SendEmailAsync(contact.User.Email, contact.Subject, path);
	}
}