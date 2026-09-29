# -*- coding: utf-8 -*-
"""
build_report.py
Trình điều phối chính biên soạn Báo cáo Đồ án môn học: Lập trình ứng dụng .NET
Đề tài: Ứng dụng phát nhạc Desktop MusicApp (.NET Framework 4.6.1 / WPF / MVVM / BFF / NAudio DSP / SQLite)

Xuất ra 2 định dạng:
  1. BAO_CAO_DO_AN_MUSICAPP.docx (Word chuẩn thể thức quy định: Times New Roman 13, lề 3.2-2-2-2cm, caption trên/dưới)
  2. BAO_CAO_DO_AN_MUSICAPP.md (Bản Markdown học thuật hoàn chỉnh)
"""

import os
import sys
from report_gen.docx_builder import DocxReportBuilder
from report_gen.front_matter import render_front_matter
from report_gen.chapter1 import render_chapter_1
from report_gen.chapter2 import render_chapter_2
from report_gen.chapter3 import render_chapter_3
from report_gen.back_matter import render_back_matter

def main():
    print("=" * 70)
    print("KHỞI TẠO TIẾN TRÌNH BIÊN SOẠN BÁO CÁO ĐỒ ÁN .NET - MUSICAPP")
    print("=" * 70)

    builder = DocxReportBuilder()
    md_lines = []

    print("-> Đang dựng Các trang đầu (Nhận xét GV, Cam đoan, Cảm ơn, Mục lục, Từ viết tắt, Danh mục bảng/hình)...")
    render_front_matter(builder, md_lines)

    print("-> Đang dựng CHƯƠNG 1: Tổng quan đề tài và Công nghệ áp dụng (1.1, 1.2, 1.3)...")
    render_chapter_1(builder, md_lines)

    print("-> Đang dựng CHƯƠNG 2: Phân tích, thiết kế hệ thống và Cơ sở dữ liệu (2.1, 2.2, 2.3)...")
    render_chapter_2(builder, md_lines)

    print("-> Đang dựng CHƯƠNG 3: Cài đặt thực nghiệm và Kết quả đạt được (3.1, 3.2, 3.3, 3.4)...")
    render_chapter_3(builder, md_lines)

    print("-> Đang dựng Phần kết thúc (Kết luận, Tài liệu tham khảo, Phụ lục A, B, C)...")
    render_back_matter(builder, md_lines)

    # Xuất tệp Word (.docx)
    docx_path = "BAO_CAO_DO_AN_MUSICAPP.docx"
    print(f"-> Đang lưu tệp Word: {docx_path}...")
    builder.save(docx_path)

    # Xuất tệp Markdown (.md)
    md_path = "BAO_CAO_DO_AN_MUSICAPP.md"
    print(f"-> Đang lưu tệp Markdown: {md_path}...")
    with open(md_path, "w", encoding="utf-8") as f:
        f.writelines(md_lines)

    docx_size = os.path.getsize(docx_path)
    md_size = os.path.getsize(md_path)
    md_line_count = len(open(md_path, "r", encoding="utf-8").readlines())

    print("=" * 70)
    print("HOÀN TẤT BIÊN SOẠN BÁO CÁO ĐỒ ÁN THÀNH CÔNG!")
    print(f"  - Tệp Word (.docx):     {docx_path} (Kích thước: {docx_size:,} bytes)")
    print(f"  - Tệp Markdown (.md):   {md_path} (Kích thước: {md_size:,} bytes, {md_line_count:,} dòng)")
    print("=" * 70)

if __name__ == "__main__":
    main()
