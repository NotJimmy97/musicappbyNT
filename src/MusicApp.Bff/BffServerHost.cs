using System;
using Microsoft.Owin.Hosting;

// OWNS: Quản lý vòng đời của OWIN Self-host server.
// DOES NOT OWN: Logic routing, controller, hoặc kết nối mạng chi tiết.
// CONSTRAINTS: Chạy cùng process với WPF UI, chỉ lắng nghe trên localhost.

namespace MusicApp.Bff
{
    /// <summary>
    /// Quản lý khởi chạy máy chủ Backend For Frontend (BFF).
    /// </summary>
    /// <remarks>
    /// 1. Trách nhiệm: Khởi tạo và lưu trữ instance của máy chủ OWIN chạy ngầm.
    /// 2. Không chịu trách nhiệm: Xử lý request HTTP, routing, hay xác thực.
    /// 3. Vòng đời trạng thái: Tồn tại suốt thời gian ứng dụng WPF chạy (process-level).
    /// 4. Yêu cầu đặc biệt: Trả về IDisposable cần được giải phóng khi ứng dụng tắt.
    /// </remarks>
    public static class BffServerHost
    {
        /// <summary>
        /// Bat dau khoi chay may chu OWIN Web API tai dia chi baseAddress chi dinh.
        /// </summary>
        /// <param name="baseAddress">Dia chi URL va cong lang nghe (mac dinh: http://localhost:5245).</param>
        /// <returns>Doi tuong IDisposable dai dien cho may chu web dang chay.</returns>
        public static IDisposable Start(string baseAddress = "http://localhost:5245")
        {
            return WebApp.Start<Startup>(baseAddress);
        }
    }
}
