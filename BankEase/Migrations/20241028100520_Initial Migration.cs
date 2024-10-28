using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankEase.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CUSTOMER",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CUSTOMER_NUMBER = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TITLE = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FIRST_NAME = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LAST_NAME = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    STREET = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CITY = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ZIPCODE = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ACCOUNT",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IBAN = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BALANCE = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    OVERDRAFT = table.Column<decimal>(type: "decimal(7,2)", nullable: false),
                    CUSTOMER_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNT", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ACCOUNT_CUSTOMER_CUSTOMER_ID",
                        column: x => x.CUSTOMER_ID,
                        principalTable: "CUSTOMER",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACTION_RECORD",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TYPE = table.Column<string>(type: "char(1)", nullable: false),
                    TEXT = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AMOUNT = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    TRANSACTION_TIME = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    ACCOUNT_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTION_RECORD", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TRANSACTION_RECORD_ACCOUNT_ACCOUNT_ID",
                        column: x => x.ACCOUNT_ID,
                        principalTable: "ACCOUNT",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNT_CUSTOMER_ID",
                table: "ACCOUNT",
                column: "CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNT_IBAN",
                table: "ACCOUNT",
                column: "IBAN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CUSTOMER_CUSTOMER_NUMBER",
                table: "CUSTOMER",
                column: "CUSTOMER_NUMBER",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_RECORD_ACCOUNT_ID",
                table: "TRANSACTION_RECORD",
                column: "ACCOUNT_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TRANSACTION_RECORD");

            migrationBuilder.DropTable(
                name: "ACCOUNT");

            migrationBuilder.DropTable(
                name: "CUSTOMER");
        }
    }
}
