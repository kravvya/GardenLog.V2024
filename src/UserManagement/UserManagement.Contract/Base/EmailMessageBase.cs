namespace UserManagement.Contract.Base;

public abstract record EmailMessageBase
{
    public string Name { get; set; }=string.Empty;
    public string Subject { get; set; }= string.Empty;
    public string Message { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    
}