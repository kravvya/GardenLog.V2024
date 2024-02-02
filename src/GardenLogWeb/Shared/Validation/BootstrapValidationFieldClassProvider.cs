using Microsoft.AspNetCore.Components.Forms;

namespace GardenLogWeb.Shared.Validation;

public class BootstrapValidationFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();

        if (editContext.IsModified(fieldIdentifier))
        {
            return isValid ? "modified is-valid" : "modified is-invalid";
        }

        return isValid ? string.Empty : "is-invalid";

    }
}
