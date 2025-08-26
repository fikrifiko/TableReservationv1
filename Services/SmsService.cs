using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System;

public class SmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    public SmsService(IConfiguration configuration)
    {
        // Read from configuration or environment variables
        _accountSid = configuration["Twilio:AccountSid"] ?? Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? string.Empty;
        _authToken = configuration["Twilio:AuthToken"] ?? Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? string.Empty;
        _twilioPhoneNumber = configuration["Twilio:PhoneNumber"] ?? Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_accountSid) || string.IsNullOrWhiteSpace(_authToken))
        {
            throw new InvalidOperationException("Twilio configuration is missing. Set Twilio:AccountSid and Twilio:AuthToken.");
        }

        TwilioClient.Init(_accountSid, _authToken);
    }

    public async Task SendSmsAsync(string recipientPhoneNumber, string message)
    {


        var messageOptions = new CreateMessageOptions(
            new PhoneNumber(recipientPhoneNumber))
        {
            From = new PhoneNumber(_twilioPhoneNumber),
            Body = message
        };

        try
        {
            var smsMessage = await MessageResource.CreateAsync(messageOptions);
            Console.WriteLine($"SMS envoyé avec succès : {smsMessage.Sid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi du SMS : {ex.Message}");
            throw;
        }
    }
}
