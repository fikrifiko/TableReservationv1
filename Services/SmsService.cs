using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Threading.Tasks;

public class SmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    public SmsService()
    {
        // Remplace par tes identifiants Twilio
        _accountSid = "ACf42f9761e76841a218cb9e6bda8581d3";
        _authToken = "6bda6c5fcbc43ac6a58e39f5416996c9";
        _twilioPhoneNumber = "+16087953466";

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
