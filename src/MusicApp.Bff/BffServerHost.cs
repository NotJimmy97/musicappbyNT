using System;
using Microsoft.Owin.Hosting;

namespace MusicApp.Bff
{
    /// <summary>
    /// Lop quan ly khoi chay may chu Backend For Frontend (BFF Host Manager).
    /// 
    /// Tac dung:
    /// - Khoi tao va luu tru may chu web nhung (Self-hosted OWIN Server) chay ngay ben trong tien trinh cua ung dung WPF.
    /// - Lang nghe cac ket noi HTTP cuc bo tren dia chi http://localhost:5245 de phuc vu cac yeu cau API va audio stream.
    /// 
    /// Van de giai quyet:
    /// - Khong can cai dat IIS hay web server ben ngoai, giup ung dung WPF co the chay doc lap (Self-contained) tren moi may tinh.
    /// - Tao ra lop trung gian co lap giao dien WPF khoi cac dich vu web ben ngoai, quan ly tap trung viec proxy stream va cache du lieu.
    /// 
    /// Cach thuc van hanh:
    /// - Su dung Microsoft.Owin.Hosting.WebApp.Start de nap lop cau hinh Startup.
    /// - Tra ve doi tuong IDisposable de App.xaml.cs giai phong khi tat ung dung.
    /// </summary>
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
