using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkCare_IT15.Data.Migrations
{
    public partial class AddBillingAndTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // === Create Billings Table ===
            migrationBuilder.CreateTable(
                name: "Billings",
                columns: table => new
                {
                    BillingID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    PatientID = table.Column<string>(nullable: true),
                    WalkInName = table.Column<string>(nullable: true),
                    AppointmentId = table.Column<int>(nullable: true),

                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billings", x => x.BillingID);

                    table.ForeignKey(
                        name: "FK_Billings_AspNetUsers_PatientID",
                        column: x => x.PatientID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");

                    table.ForeignKey(
                        name: "FK_Billings_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                });

            // === Create Transactions Table ===
            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    BillingID = table.Column<int>(nullable: false),

                    // 💰 Payment Details
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Change = table.Column<decimal>(type: "decimal(18,2)", nullable: false),

                    TransactionDate = table.Column<DateTime>(nullable: false),

                    TransactionType = table.Column<string>(nullable: false),
                    PaymentMethod = table.Column<string>(nullable: false),

                    ReferenceNumber = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionID);

                    table.ForeignKey(
                        name: "FK_Transactions_Billings_BillingID",
                        column: x => x.BillingID,
                        principalTable: "Billings",
                        principalColumn: "BillingID",
                        onDelete: ReferentialAction.Cascade);
                });

            // === Indexes ===
            migrationBuilder.CreateIndex(
                name: "IX_Billings_PatientID",
                table: "Billings",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Billings_AppointmentId",
                table: "Billings",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BillingID",
                table: "Transactions",
                column: "BillingID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Transactions");
            migrationBuilder.DropTable(name: "Billings");
        }
    }
}
