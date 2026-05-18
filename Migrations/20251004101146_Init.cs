using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDH.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KHACHHANG",
                columns: table => new
                {
                    MAKH = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    TENKH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DIACHI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SDT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MALOAIKH = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHACHHANG", x => x.MAKH);
                });

            migrationBuilder.CreateTable(
                name: "LoaiBaoCao",
                columns: table => new
                {
                    MALOAIBC = table.Column<string>(type: "NVARCHAR2(6)", maxLength: 6, nullable: false),
                    TENLOAIBC = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiBaoCao", x => x.MALOAIBC);
                });

            migrationBuilder.CreateTable(
                name: "LoaiKhachHang",
                columns: table => new
                {
                    MALOAIKH = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    TENLOAIKH = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiKhachHang", x => x.MALOAIKH);
                });

            migrationBuilder.CreateTable(
                name: "LoaiSanPham",
                columns: table => new
                {
                    MALOAISP = table.Column<string>(type: "NVARCHAR2(6)", maxLength: 6, nullable: false),
                    TENLOAI = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSanPham", x => x.MALOAISP);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungCap",
                columns: table => new
                {
                    MANCC = table.Column<string>(type: "NVARCHAR2(6)", maxLength: 6, nullable: false),
                    TENNCC = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DIACHI = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    SDT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungCap", x => x.MANCC);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MANV = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    TENNV = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CHUCVU = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    DIACHI = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    SDT = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    EMAIL = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MANV);
                });

            migrationBuilder.CreateTable(
                name: "TAIKHOAN",
                columns: table => new
                {
                    MANV = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    MATKHAU = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    VAITRO = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAIKHOAN", x => x.MANV);
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    MASP = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    TENSP = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    GIANHAP = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    GIABAN = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    SOLUONGTON = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    GHICHU = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    NGAYTAO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MANCC = table.Column<string>(type: "NVARCHAR2(6)", nullable: false),
                    MALOAISP = table.Column<string>(type: "NVARCHAR2(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.MASP);
                    table.ForeignKey(
                        name: "FK_SanPham_LoaiSanPham_MALOAISP",
                        column: x => x.MALOAISP,
                        principalTable: "LoaiSanPham",
                        principalColumn: "MALOAISP",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPham_NhaCungCap_MANCC",
                        column: x => x.MANCC,
                        principalTable: "NhaCungCap",
                        principalColumn: "MANCC",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BaoCao",
                columns: table => new
                {
                    MABC = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    TENBC = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    MALOAIBC = table.Column<string>(type: "NVARCHAR2(6)", nullable: false),
                    TUNGAY = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TOINGAY = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    GHICHU = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    NGAYLAP = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    MANV = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCao", x => x.MABC);
                    table.ForeignKey(
                        name: "FK_BaoCao_LoaiBaoCao_MALOAIBC",
                        column: x => x.MALOAIBC,
                        principalTable: "LoaiBaoCao",
                        principalColumn: "MALOAIBC",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCao_NhanVien_MANV",
                        column: x => x.MANV,
                        principalTable: "NhanVien",
                        principalColumn: "MANV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonHang",
                columns: table => new
                {
                    MADH = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    NGAYLAP = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    NGAYHT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    TRANGTHAI = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    MAKH = table.Column<string>(type: "NVARCHAR2(450)", nullable: false),
                    MANV = table.Column<string>(type: "NVARCHAR2(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHang", x => x.MADH);
                    table.ForeignKey(
                        name: "FK_DonHang_KHACHHANG_MAKH",
                        column: x => x.MAKH,
                        principalTable: "KHACHHANG",
                        principalColumn: "MAKH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DonHang_NhanVien_MANV",
                        column: x => x.MANV,
                        principalTable: "NhanVien",
                        principalColumn: "MANV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHang",
                columns: table => new
                {
                    MADH = table.Column<string>(type: "NVARCHAR2(5)", nullable: false),
                    MASP = table.Column<string>(type: "NVARCHAR2(5)", nullable: false),
                    SOLUONG = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DONGIA = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    VAT = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TONGTIEN = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHang", x => new { x.MADH, x.MASP });
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_DonHang_MADH",
                        column: x => x.MADH,
                        principalTable: "DonHang",
                        principalColumn: "MADH",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHang_SanPham_MASP",
                        column: x => x.MASP,
                        principalTable: "SanPham",
                        principalColumn: "MASP",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MAHD = table.Column<string>(type: "NVARCHAR2(5)", maxLength: 5, nullable: false),
                    MADH = table.Column<string>(type: "NVARCHAR2(5)", nullable: false),
                    NGAYLAP = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    PHUONGTHUCTT = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    TONGTIENCHUAVAT = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TIENVAT = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false),
                    TONGTIEN = table.Column<decimal>(type: "DECIMAL(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MAHD);
                    table.ForeignKey(
                        name: "FK_HoaDon_DonHang_MADH",
                        column: x => x.MADH,
                        principalTable: "DonHang",
                        principalColumn: "MADH",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_MALOAIBC",
                table: "BaoCao",
                column: "MALOAIBC");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCao_MANV",
                table: "BaoCao",
                column: "MANV");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHang_MASP",
                table: "ChiTietDonHang",
                column: "MASP");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MAKH",
                table: "DonHang",
                column: "MAKH");

            migrationBuilder.CreateIndex(
                name: "IX_DonHang_MANV",
                table: "DonHang",
                column: "MANV");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MADH",
                table: "HoaDon",
                column: "MADH");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MALOAISP",
                table: "SanPham",
                column: "MALOAISP");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MANCC",
                table: "SanPham",
                column: "MANCC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCao");

            migrationBuilder.DropTable(
                name: "ChiTietDonHang");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "LoaiKhachHang");

            migrationBuilder.DropTable(
                name: "TAIKHOAN");

            migrationBuilder.DropTable(
                name: "LoaiBaoCao");

            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "DonHang");

            migrationBuilder.DropTable(
                name: "LoaiSanPham");

            migrationBuilder.DropTable(
                name: "NhaCungCap");

            migrationBuilder.DropTable(
                name: "KHACHHANG");

            migrationBuilder.DropTable(
                name: "NhanVien");
        }
    }
}
