using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq.Expressions;

namespace GardenLogWeb.Shared.Controls;

public class BS5ValidationMessage<TValue> : ComponentBase, IDisposable
{
    private EditContext? _previousEditContext;
    private Expression<Func<TValue>>? _previousFieldAccessor;
    private readonly EventHandler<ValidationStateChangedEventArgs>? _validationStateChangedHandler;
    private FieldIdentifier? _fieldIdentifier;

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created <c>div</c> element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter] EditContext? CurrentEditContext { get; set; }

    /// <summary>
    /// Specifies the field for which validation messages should be displayed.
    /// </summary>
    [Parameter] public Expression<Func<TValue>>? For { get; set; }

    /// <summary>`
    /// Constructs an instance of <see cref="ValidationMessage{TValue}"/>.
    /// </summary>
    public BS5ValidationMessage()
    {
        _validationStateChangedHandler = (sender, eventArgs) => StateHasChanged();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                $"of type {nameof(EditContext)}. For example, you can use {GetType()} inside " +
                $"an {nameof(EditForm)}.");
        }

        if (For == null) // Not possible except if you manually specify T
        {
            throw new InvalidOperationException($"{GetType()} requires a value for the " +
                $"{nameof(For)} parameter.");
        }
        else if (For != _previousFieldAccessor)
        {
            _fieldIdentifier = FieldIdentifier.Create(For);
            _previousFieldAccessor = For;
        }

        if (CurrentEditContext != _previousEditContext)
        {
            DetachValidationStateChangedListener();
            CurrentEditContext.OnValidationStateChanged += _validationStateChangedHandler;
            _previousEditContext = CurrentEditContext;
        }
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (CurrentEditContext == null || !_fieldIdentifier.HasValue) return;

        foreach (var message in CurrentEditContext.GetValidationMessages(_fieldIdentifier.Value))
        {
            //this is a root div. 
            builder.OpenElement(0, "div");
            //this div is to play failed input control role. 
            builder.OpenElement(1, "div");   
            builder.AddAttribute(2, "class", "is-invalid");
            //"failed" input control needs to be closed before the div with feedback.
            builder.CloseElement();
            builder.OpenElement(3, "div");
            builder.AddAttribute(4, "class", "invalid-feedback");
            builder.OpenElement(5, "div");
            builder.AddMultipleAttributes(6, AdditionalAttributes);
            builder.AddAttribute(7, "class", "validation-message");
            builder.AddContent(8, message);
            builder.CloseElement();
            builder.CloseElement();
            builder.CloseElement();

        }
    }

    private void HandleValidationStateChanged(object sender, ValidationStateChangedEventArgs eventArgs)
    {
        StateHasChanged();
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    void IDisposable.Dispose()
    {
        DetachValidationStateChangedListener();
        Dispose(disposing: true);
    }

    private void DetachValidationStateChangedListener()
    {
        if (_previousEditContext != null)
        {
            _previousEditContext.OnValidationStateChanged -= _validationStateChangedHandler;
        }
    }
}
