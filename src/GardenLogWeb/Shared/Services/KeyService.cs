using Microsoft.AspNetCore.Components.Web;

namespace GardenLogWeb.Shared.Services
{
    public interface IKeyService
    {
        event EventHandler<KeyboardEventArgs>? OnKeyDown;
        void KeyDown(object obj, KeyboardEventArgs evt);
    }

    public class KeyService : IKeyService
    {
        public event EventHandler<KeyboardEventArgs>? OnKeyDown;

        public void KeyDown(object obj, KeyboardEventArgs evt) => OnKeyDown?.Invoke(obj, evt);
    }
}
