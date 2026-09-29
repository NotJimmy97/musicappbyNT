# -*- coding: utf-8 -*-
"""
docx_builder.py
Bộ công cụ dựng file Word (.docx) chuẩn quy cách văn bản kỹ thuật & đồ án tốt nghiệp:
- Font: Times New Roman, cỡ 13pt cho phần thân.
- Giãn dòng: 1.4 lines, Before: 3pt, After: 6pt.
- Lề trang: Trái: 3.2 cm, Phải: 2.0 cm, Trên: 2.0 cm, Dưới: 2.0 cm.
- Caption bảng: Bên TRÊN bảng, in nghiêng/đậm.
- Caption hình: Bên DƯỚI hình, căn giữa, in nghiêng.
- Hộp Code Snippet: Font Consolas 9.5pt, nền xám nhạt #F4F5F7, viền đơn #D0D5DD.
- Đánh số trang tự động ở Footer.
"""

import docx
from docx.shared import Inches, Pt, RGBColor, Cm
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_ALIGN_VERTICAL
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

COLOR_PRIMARY = "1F4E79"       # Xanh đậm kỹ thuật
COLOR_SECONDARY = "2C3E50"     # Xám đậm
COLOR_TEXT = "000000"          # Đen chuẩn
COLOR_MUTED = "555555"         # Xám phụ
COLOR_BORDER = "CCCCCC"        # Viền bảng
COLOR_BG_HEADER = "1F4E79"     # Nền header bảng
COLOR_BG_ALT = "F9FAFB"        # Nền dòng so le bảng
COLOR_BG_CODE = "F4F5F7"       # Nền hộp code
COLOR_BORDER_CODE = "D0D5DD"   # Viền hộp code

class DocxReportBuilder:
    def __init__(self):
        self.doc = docx.Document()
        self._setup_page_layout()
        self._setup_styles()
        self._setup_header_footer()

    def _setup_page_layout(self):
        """Cấu hình lề A4: Trái 3.2cm, Phải 2.0cm, Trên 2.0cm, Dưới 2.0cm"""
        section = self.doc.sections[0]
        section.page_width = Cm(21.0)
        section.page_height = Cm(29.7)
        section.top_margin = Cm(2.0)
        section.bottom_margin = Cm(2.0)
        section.left_margin = Cm(3.2)
        section.right_margin = Cm(2.0)

    def _setup_styles(self):
        """Cấu hình style mặc định Normal và Headings"""
        normal_style = self.doc.styles['Normal']
        normal_style.font.name = 'Times New Roman'
        normal_style.font.size = Pt(13)
        normal_style.font.color.rgb = RGBColor(0, 0, 0)
        normal_style.paragraph_format.line_spacing = 1.4
        normal_style.paragraph_format.space_before = Pt(3)
        normal_style.paragraph_format.space_after = Pt(6)

    def _setup_header_footer(self):
        """Cấu hình Running Header và Footer số trang tự động"""
        section = self.doc.sections[0]
        header = section.header
        hp = header.paragraphs[0]
        hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        hrun = hp.add_run("Báo cáo Đồ án: Ứng dụng phát nhạc Desktop MusicApp .NET/WPF")
        hrun.font.name = 'Times New Roman'
        hrun.font.size = Pt(9.5)
        hrun.font.italic = True
        hrun.font.color.rgb = RGBColor(128, 128, 128)

        footer = section.footer
        fp = footer.paragraphs[0]
        fp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        frun1 = fp.add_run("Trang ")
        frun1.font.name = 'Times New Roman'
        frun1.font.size = Pt(10)
        frun1.font.color.rgb = RGBColor(100, 100, 100)
        self._add_page_number_field(frun1)

    def _add_page_number_field(self, run):
        """Thêm mã trường PAGE tự động của Word"""
        fldChar1 = OxmlElement('w:fldChar')
        fldChar1.set(qn('w:fldCharType'), 'begin')
        instrText = OxmlElement('w:instrText')
        instrText.set(qn('xml:space'), 'preserve')
        instrText.text = 'PAGE'
        fldChar2 = OxmlElement('w:fldChar')
        fldChar2.set(qn('w:fldCharType'), 'separate')
        fldChar3 = OxmlElement('w:fldChar')
        fldChar3.set(qn('w:fldCharType'), 'end')
        r = run._r
        r.append(fldChar1)
        r.append(instrText)
        r.append(fldChar2)
        r.append(fldChar3)

    def add_page_break(self):
        self.doc.add_page_break()

    def add_title(self, text, subtitle=None):
        """Tiêu đề lớn của báo cáo"""
        p = self.doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_before = Pt(24)
        p.paragraph_format.space_after = Pt(12)
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(22)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)

        if subtitle:
            p2 = self.doc.add_paragraph()
            p2.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p2.paragraph_format.space_before = Pt(0)
            p2.paragraph_format.space_after = Pt(24)
            run2 = p2.add_run(subtitle)
            run2.font.name = 'Times New Roman'
            run2.font.size = Pt(14)
            run2.font.italic = True
            run2.font.color.rgb = RGBColor(0x55, 0x55, 0x55)

    def add_heading_1(self, text, page_break=True):
        """Tiêu đề Chương (Heading 1)"""
        if page_break and len(self.doc.paragraphs) > 1:
            self.doc.add_page_break()
        p = self.doc.add_paragraph()
        p.paragraph_format.space_before = Pt(16)
        p.paragraph_format.space_after = Pt(8)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(17)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)
        return p

    def add_heading_2(self, text):
        """Tiêu đề Mục lớn (Heading 2, vd: 1.1, 2.1)"""
        p = self.doc.add_paragraph()
        p.paragraph_format.space_before = Pt(12)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(14.5)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)
        return p

    def add_heading_3(self, text):
        """Tiêu đề Mục con (Heading 3, vd: 1.1.1, 2.1.1)"""
        p = self.doc.add_paragraph()
        p.paragraph_format.space_before = Pt(8)
        p.paragraph_format.space_after = Pt(3)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(13)
        run.font.bold = True
        run.font.color.rgb = RGBColor(0x2C, 0x3E, 0x50)
        return p

    def add_heading_4(self, text):
        """Tiêu đề Mục chi tiết (Heading 4, vd: 1.1.1.1 hoặc điểm nhấn a, b)"""
        p = self.doc.add_paragraph()
        p.paragraph_format.space_before = Pt(6)
        p.paragraph_format.space_after = Pt(2)
        p.paragraph_format.keep_with_next = True
        run = p.add_run(text)
        run.font.name = 'Times New Roman'
        run.font.size = Pt(13)
        run.font.bold = True
        run.font.italic = True
        run.font.color.rgb = RGBColor(0x33, 0x33, 0x33)
        return p

    def add_paragraph(self, text, bold_prefix=None, italic=False, align=WD_ALIGN_PARAGRAPH.JUSTIFY):
        """Thêm đoạn văn bản chuẩn, căn đều hai bên"""
        p = self.doc.add_paragraph()
        p.alignment = align
        p.paragraph_format.line_spacing = 1.4
        p.paragraph_format.space_before = Pt(3)
        p.paragraph_format.space_after = Pt(6)

        if bold_prefix:
            r_bold = p.add_run(bold_prefix)
            r_bold.font.name = 'Times New Roman'
            r_bold.font.size = Pt(13)
            r_bold.font.bold = True

        r = p.add_run(text)
        r.font.name = 'Times New Roman'
        r.font.size = Pt(13)
        r.font.italic = italic
        return p

    def add_bullet_point(self, text, bold_prefix=None, level=0):
        """Thêm mục danh sách đầu dòng"""
        p = self.doc.add_paragraph(style='List Bullet')
        p.paragraph_format.line_spacing = 1.3
        p.paragraph_format.space_before = Pt(2)
        p.paragraph_format.space_after = Pt(4)
        p.paragraph_format.left_indent = Inches(0.25 * (level + 1))

        if bold_prefix:
            r_bold = p.add_run(bold_prefix)
            r_bold.font.name = 'Times New Roman'
            r_bold.font.size = Pt(13)
            r_bold.font.bold = True

        r = p.add_run(text)
        r.font.name = 'Times New Roman'
        r.font.size = Pt(13)
        return p

    def add_table(self, caption, headers, rows, col_widths=None):
        """
        Thêm bảng dữ liệu chuẩn kỹ thuật:
        - Caption nằm PHÍA TRÊN bảng, in đậm/nghiêng.
        - Header bảng màu #1F4E79, chữ trắng đậm.
        - Dòng so le #F9FAFB.
        - Viền nhẹ #CCCCCC.
        """
        # Caption phía trên bảng
        cp = self.doc.add_paragraph()
        cp.paragraph_format.space_before = Pt(8)
        cp.paragraph_format.space_after = Pt(3)
        cp.paragraph_format.keep_with_next = True
        c_run = cp.add_run(caption)
        c_run.font.name = 'Times New Roman'
        c_run.font.size = Pt(12)
        c_run.font.bold = True
        c_run.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)

        # Tạo bảng
        table = self.doc.add_table(rows=len(rows) + 1, cols=len(headers))
        table.alignment = WD_TABLE_ALIGNMENT.CENTER
        tblPr = table._tbl.tblPr
        borders = parse_xml(
            f'<w:tblBorders {nsdecls("w")}>'
            f'<w:top w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'<w:bottom w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'<w:insideH w:val="single" w:sz="4" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'<w:insideV w:val="single" w:sz="4" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'<w:left w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'<w:right w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER}"/>'
            f'</w:tblBorders>'
        )
        tblPr.append(borders)

        # Header Row
        hdr_row = table.rows[0]
        hdr_row._tr.get_or_add_trPr().append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))
        for idx, title in enumerate(headers):
            cell = hdr_row.cells[idx]
            tcPr = cell._tc.get_or_add_tcPr()
            tcPr.append(parse_xml(f'<w:shd {nsdecls("w")} w:fill="{COLOR_BG_HEADER}"/>'))
            tcPr.append(parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="120" w:type="dxa"/><w:bottom w:w="120" w:type="dxa"/><w:left w:w="140" w:type="dxa"/><w:right w:w="140" w:type="dxa"/></w:tcMar>'))
            p = cell.paragraphs[0]
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p.paragraph_format.space_before = Pt(2)
            p.paragraph_format.space_after = Pt(2)
            r = p.add_run(title)
            r.font.name = 'Times New Roman'
            r.font.size = Pt(11)
            r.font.bold = True
            r.font.color.rgb = RGBColor(255, 255, 255)

        # Data Rows
        for r_idx, row_data in enumerate(rows):
            row = table.rows[r_idx + 1]
            bg_color = COLOR_BG_ALT if (r_idx % 2 == 1) else "FFFFFF"
            for c_idx, val in enumerate(row_data):
                cell = row.cells[c_idx]
                tcPr = cell._tc.get_or_add_tcPr()
                if bg_color != "FFFFFF":
                    tcPr.append(parse_xml(f'<w:shd {nsdecls("w")} w:fill="{bg_color}"/>'))
                tcPr.append(parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="100" w:type="dxa"/><w:bottom w:w="100" w:type="dxa"/><w:left w:w="140" w:type="dxa"/><w:right w:w="140" w:type="dxa"/></w:tcMar>'))
                p = cell.paragraphs[0]
                p.alignment = WD_ALIGN_PARAGRAPH.LEFT if c_idx > 0 else WD_ALIGN_PARAGRAPH.CENTER
                p.paragraph_format.line_spacing = 1.2
                p.paragraph_format.space_before = Pt(2)
                p.paragraph_format.space_after = Pt(2)
                r = p.add_run(str(val))
                r.font.name = 'Times New Roman'
                r.font.size = Pt(11)

        # Căn chỉnh độ rộng cột nếu có
        if col_widths and len(col_widths) == len(headers):
            for row in table.rows:
                for c_idx, width in enumerate(col_widths):
                    row.cells[c_idx].width = width

        # Đoạn trắng đệm sau bảng
        p_after = self.doc.add_paragraph()
        p_after.paragraph_format.space_before = Pt(2)
        p_after.paragraph_format.space_after = Pt(6)

    def add_figure_placeholder(self, caption, schematic_text, note=None):
        """
        Thêm khung hình vẽ minh họa / sơ đồ kỹ thuật:
        - Khung hộp có viền kép hoặc viền đơn mô tả cấu trúc hình vẽ / mockup.
        - Caption nằm PHÍA DƯỚI hình, căn giữa, in nghiêng.
        """
        table = self.doc.add_table(rows=1, cols=1)
        table.alignment = WD_TABLE_ALIGNMENT.CENTER
        cell = table.cell(0, 0)
        cell.width = Cm(15.5)

        tcPr = cell._tc.get_or_add_tcPr()
        tcPr.append(parse_xml(f'<w:shd {nsdecls("w")} w:fill="F8F9FA"/>'))
        tcPr.append(parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="180" w:type="dxa"/><w:bottom w:w="180" w:type="dxa"/><w:left w:w="220" w:type="dxa"/><w:right w:w="220" w:type="dxa"/></w:tcMar>'))
        
        tblPr = table._tbl.tblPr
        tblPr.append(parse_xml(
            f'<w:tblBorders {nsdecls("w")}>'
            f'<w:top w:val="single" w:sz="8" w:space="0" w:color="A0B2C6"/>'
            f'<w:bottom w:val="single" w:sz="8" w:space="0" w:color="A0B2C6"/>'
            f'<w:left w:val="single" w:sz="8" w:space="0" w:color="A0B2C6"/>'
            f'<w:right w:val="single" w:sz="8" w:space="0" w:color="A0B2C6"/>'
            f'</w:tblBorders>'
        ))

        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_before = Pt(4)
        p.paragraph_format.space_after = Pt(4)
        
        run = p.add_run(schematic_text)
        run.font.name = 'Consolas'
        run.font.size = Pt(9.5)
        run.font.color.rgb = RGBColor(0x2C, 0x3E, 0x50)

        if note:
            p_note = cell.add_paragraph()
            p_note.alignment = WD_ALIGN_PARAGRAPH.CENTER
            p_note.paragraph_format.space_before = Pt(2)
            p_note.paragraph_format.space_after = Pt(2)
            r_note = p_note.add_run(f"[{note}]")
            r_note.font.name = 'Times New Roman'
            r_note.font.size = Pt(10)
            r_note.font.italic = True
            r_note.font.color.rgb = RGBColor(0x7F, 0x8C, 0x8D)

        # Caption hình bên DƯỚI
        cp = self.doc.add_paragraph()
        cp.alignment = WD_ALIGN_PARAGRAPH.CENTER
        cp.paragraph_format.space_before = Pt(4)
        cp.paragraph_format.space_after = Pt(10)
        c_run = cp.add_run(caption)
        c_run.font.name = 'Times New Roman'
        c_run.font.size = Pt(12)
        c_run.font.italic = True
        c_run.font.bold = True
        c_run.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)

    def add_code_snippet(self, title, code_text):
        """
        Thêm khung trích đoạn mã nguồn then chốt:
        - Tiêu đề khung code (Consolas đậm).
        - Font Consolas 9.5pt, nền xám nhạt #F4F5F7, viền đơn #D0D5DD.
        """
        table = self.doc.add_table(rows=1, cols=1)
        table.alignment = WD_TABLE_ALIGNMENT.CENTER
        cell = table.cell(0, 0)
        cell.width = Cm(15.5)

        tcPr = cell._tc.get_or_add_tcPr()
        tcPr.append(parse_xml(f'<w:shd {nsdecls("w")} w:fill="{COLOR_BG_CODE}"/>'))
        tcPr.append(parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="140" w:type="dxa"/><w:bottom w:w="140" w:type="dxa"/><w:left w:w="180" w:type="dxa"/><w:right w:w="180" w:type="dxa"/></w:tcMar>'))
        
        tblPr = table._tbl.tblPr
        tblPr.append(parse_xml(
            f'<w:tblBorders {nsdecls("w")}>'
            f'<w:top w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER_CODE}"/>'
            f'<w:bottom w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER_CODE}"/>'
            f'<w:left w:val="single" w:sz="18" w:space="0" w:color="{COLOR_PRIMARY}"/>'
            f'<w:right w:val="single" w:sz="6" w:space="0" w:color="{COLOR_BORDER_CODE}"/>'
            f'</w:tblBorders>'
        ))

        p_hdr = cell.paragraphs[0]
        p_hdr.paragraph_format.line_spacing = 1.15
        p_hdr.paragraph_format.space_before = Pt(2)
        p_hdr.paragraph_format.space_after = Pt(4)
        r_hdr = p_hdr.add_run(f"// --- {title} ---")
        r_hdr.font.name = 'Consolas'
        r_hdr.font.size = Pt(9.5)
        r_hdr.font.bold = True
        r_hdr.font.color.rgb = RGBColor(0x1F, 0x4E, 0x79)

        p_code = cell.add_paragraph()
        p_code.paragraph_format.line_spacing = 1.15
        p_code.paragraph_format.space_before = Pt(2)
        p_code.paragraph_format.space_after = Pt(2)
        r_code = p_code.add_run(code_text)
        r_code.font.name = 'Consolas'
        r_code.font.size = Pt(9.0)
        r_code.font.color.rgb = RGBColor(0x24, 0x29, 0x2E)

        p_after = self.doc.add_paragraph()
        p_after.paragraph_format.space_before = Pt(2)
        p_after.paragraph_format.space_after = Pt(6)

    def save(self, filepath):
        self.doc.save(filepath)
        print(f"Đã lưu thành công tệp Word: {filepath}")
