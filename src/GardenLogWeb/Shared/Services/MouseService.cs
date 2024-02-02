using Microsoft.AspNetCore.Components.Web;

namespace GardenLogWeb.Shared.Services
{
    public interface IMouseService
    {
        event EventHandler<MouseEventArgs>? OnMove;
        event EventHandler<MouseEventArgs>? OnUp;
        event EventHandler<MouseEventArgs>? OnLeave;
        void MouseMove(object obj, MouseEventArgs evt);
        void MouseUp(object obj, MouseEventArgs evt);
        void MouseLeave(object obj, MouseEventArgs evt);
    }

    public class MouseService : IMouseService
    {
        public event EventHandler<MouseEventArgs>? OnMove;
        public event EventHandler<MouseEventArgs>? OnUp;
        public event EventHandler<MouseEventArgs>? OnLeave;

        public void MouseMove(object obj, MouseEventArgs evt) => OnMove?.Invoke(obj, evt);
        public void MouseUp(object obj, MouseEventArgs evt) => OnUp?.Invoke(obj, evt);
        public void MouseLeave(object obj, MouseEventArgs evt) => OnLeave?.Invoke(obj, evt);
    }
}
