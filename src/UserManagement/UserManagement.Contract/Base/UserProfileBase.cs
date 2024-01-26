namespace UserManagement.Contract.Base;

public abstract record UserProfileBase
{
    public string UserName { get; set; }=string.Empty;
    public string FirstName { get; set; }= string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    
}