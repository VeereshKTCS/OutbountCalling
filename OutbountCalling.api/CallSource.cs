using Azure.Communication;
using Azure.Communication.CallAutomation;

internal class CallSource : CallInvite
{
    public CallSource(CommunicationUserIdentifier targetIdentity) : base(targetIdentity)
    {
    }

    public PhoneNumberIdentifier CallerId { get; set; }
    public string DisplayName { get; set; }
}