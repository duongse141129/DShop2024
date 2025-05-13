using System;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Azure.Core;
using DShop2024.EnumData;
using DShop2024.Models;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Encodings;
using MimeKit.Utils;

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
    Task SendEmailTemplateAsync(string email, string subject, BodyBuilder builder);
    Task SendSmsAsync(string number, string message);
    Task SendEmailOrder(OrderModel order, InformationShopModel infoShop);
	Task SendEmailContact(ContactModel contact, InformationShopModel infoShop);
    Task SendEmailCouponForNewCustomer(AppUserModel userModel, CouponModel couponModel, InformationShopModel infoShop);
    Task SendEmailOTPconfirm(AppUserModel userModel, string otp, InformationShopModel infoShop);
    Task SendEmailOTP(AppUserModel userModel, string otp, string typeService, InformationShopModel infoShop);
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


    public async Task SendEmailTemplateAsync(string email, string subject, BodyBuilder builder)
    {
        var message = new MimeMessage();
        //var message = new MailMessage();
        message.Sender = new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail);
        message.From.Add(new MailboxAddress(mailSettings.DisplayName, mailSettings.Mail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

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

	public async Task SendEmailAsync(string email, string subject, string htmlMessage)
	{
		var message = new MimeMessage();
		//var message = new MailMessage();
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

		var builder = new BodyBuilder();
		var pathLogo = Path.Combine(webRootPath, "media\\Logo\\" + infoShop.LogoImg);
		var image = builder.LinkedResources.Add(pathLogo);
		image.ContentId = MimeUtils.GenerateMessageId();

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
		path = path.Replace("{{Logo}}", image.ContentId);

		builder.HtmlBody = path;

		await SendEmailTemplateAsync(order.User.Email, "Order DShop2024", builder);
    }


	public async Task SendEmailContact(ContactModel contact, InformationShopModel infoShop)
	{
		string webRootPath = _webHostEnvironment.WebRootPath;

		var builder = new BodyBuilder();
		var pathLogo = Path.Combine(webRootPath, "media\\Logo\\" + infoShop.LogoImg);
        var image = builder.LinkedResources.Add(pathLogo);
        image.ContentId = MimeUtils.GenerateMessageId();

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
		path = path.Replace("{{Logo}}", image.ContentId);

		builder.HtmlBody = path;

		await SendEmailTemplateAsync(contact.User.Email, contact.Subject, builder);
	}

    public async Task SendEmailOTPconfirm(AppUserModel userModel,string otp ,InformationShopModel infoShop)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;

		var builder = new BodyBuilder();
		var pathLogo = Path.Combine(webRootPath, "media\\Logo\\" + infoShop.LogoImg);
		var image = builder.LinkedResources.Add(pathLogo);
		image.ContentId = MimeUtils.GenerateMessageId();

		string path = "";
        path = System.IO.File.ReadAllText(Path.Combine(webRootPath, "media\\Email\\otp22.html"));
        path = path.Replace("{{UserName}}", userModel.UserName);
        path = path.Replace("{{OTPcode}}", otp);

        path = path.Replace("{{ShopName}}", infoShop.ShopName);
        path = path.Replace("{{EmailShop}}", infoShop.Email);
        path = path.Replace("{{HotlineShop}}", infoShop.Phone);
		path = path.Replace("{{Logo}}", image.ContentId);

		builder.HtmlBody = path;

		await SendEmailTemplateAsync(userModel.Email, "confirm email for register", builder);
    }

    public async Task SendEmailCouponForNewCustomer(AppUserModel userModel,CouponModel couponModel, InformationShopModel infoShop)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;

		var builder = new BodyBuilder();
		var pathLogo = Path.Combine(webRootPath, "media\\Logo\\" + infoShop.LogoImg);
		var image = builder.LinkedResources.Add(pathLogo);
		image.ContentId = MimeUtils.GenerateMessageId();

		string path = "";
        path = System.IO.File.ReadAllText(Path.Combine(webRootPath, "media\\Email\\sendCouponPersent.html"));
        path = path.Replace("{{UserName}}", userModel.UserName);
        path = path.Replace("{{CouponName}}", couponModel.CouponName);
        path = path.Replace("{{Description}}", couponModel.Description);
        path = path.Replace("{{CouponCode}}", couponModel.CouponCode);
        path = path.Replace("{{DateExpire}}", couponModel.DateExpired.ToShortDateString());

        path = path.Replace("{{ShopName}}", infoShop.ShopName);
        path = path.Replace("{{EmailShop}}", infoShop.Email);
        path = path.Replace("{{HotlineShop}}", infoShop.Phone);
		path = path.Replace("{{Logo}}", image.ContentId);

		builder.HtmlBody = path;

		await SendEmailTemplateAsync(userModel.Email, "Promotion for new customers", builder);
    }

    public async Task SendEmailOTP(AppUserModel userModel, string otp,string typeService, InformationShopModel infoShop)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;

		var builder = new BodyBuilder();
		var pathLogo = Path.Combine(webRootPath, "media\\Logo\\" + infoShop.LogoImg);
		var image = builder.LinkedResources.Add(pathLogo);
		image.ContentId = MimeUtils.GenerateMessageId();

		string title = "", subject = "";
        if(typeService == DShopConst.OTP_CONFIRM_EMAIL)
        {
            title = "Please enter this confirmation code in the window where you started creating your account:";
            subject = "confirm email for register";
        }
        if(typeService == DShopConst.OTP_RESET_PASSWORD)
        {
            title = "Please enter this confirmation code in the window where you want to reset password your account:";
            subject =  "confirm email for reset password";
        }

        string path = "";
        path = System.IO.File.ReadAllText(Path.Combine(webRootPath, "media\\Email\\otpEmail.html"));
        path = path.Replace("{{UserName}}", userModel.UserName);
        path = path.Replace("{{OTPcode}}", otp);

        path = path.Replace("{{Title}}", title);
        
        path = path.Replace("{{ShopName}}", infoShop.ShopName);
        path = path.Replace("{{EmailShop}}", infoShop.Email);
        path = path.Replace("{{HotlineShop}}", infoShop.Phone);
		path = path.Replace("{{Logo}}", image.ContentId);

		builder.HtmlBody = path;

		await SendEmailTemplateAsync(userModel.Email, subject, builder);
    }
}