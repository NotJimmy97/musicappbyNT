using System.Collections.Generic;

namespace MusicApp.Core.Dtos
{
    /// <summary>
    /// Doi tuong truyen tai du lieu ket qua tim kiem (Data Transfer Object) giua BFF va Client.
    /// 
    /// Tac dung:
    /// - Dong goi tap hop ket qua tim kiem tra ve tu Backend For Frontend (BFF) Web API.
    /// - Cung cap tong so ban ghi phu hop (Total) va danh sach cac bai hat cu the (Items).
    /// 
    /// Van de giai quyet:
    /// - Ngan ngua su phu thuoc chat che (Tight Coupling) giua giao dien nguoi dung WPF va cac nha cung cap API ben ngoai (Jamendo, Zing, Spotify...).
    /// - Dam bao tinh Backward Compatibility: bat ky thay doi nao tu schema cua API ben ngoai deu duoc BFF chuan hoa ve DTO nay.
    /// 
    /// Cach thuc van hanh:
    /// - Duoc serialize thanh chuoi JSON boi ASP.NET Web API (OWIN Host) tai endpoint /api/tracks/search.
    /// - MusicApiClient phia WPF nhan stream HTTP va deserialize thanh SearchResponseDto bang Newtonsoft.Json.
    /// </summary>
    public class SearchResponseDto
    {
        /// <summary>
        /// Tong so luong ban ghi tim thay thoa man tieu chi tim kiem.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Danh sach cac DTO bai hat thuoc trang ket qua hien tai.
        /// </summary>
        public List<TrackDto> Items { get; set; } = new List<TrackDto>();
    }
}
