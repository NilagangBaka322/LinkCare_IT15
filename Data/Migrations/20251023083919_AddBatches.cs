using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkCare_IT15.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Consumables");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Consumables");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "Consumables");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Consumables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConsumableBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumableId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShelfLifeDays = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_ConsumableBatches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsumableBatches_Consumables_ConsumableId",
                        column: x => x.ConsumableId,
                        principalTable: "Consumables",
                        principalColumn: "ConsumableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableBatches_ConsumableId",
                table: "ConsumableBatches",
                column: "ConsumableId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableBatches_UserId",
                table: "ConsumableBatches",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableBatches");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Consumables");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Consumables",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Consumables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "Consumables",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
