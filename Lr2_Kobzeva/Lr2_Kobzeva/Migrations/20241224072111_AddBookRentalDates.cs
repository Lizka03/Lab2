using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lr2_Kobzeva.Migrations
{
    /// <inheritdoc />
    public partial class AddBookRentalDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateRented",
                table: "Books",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Books",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRented",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Books");
        }
    }
}
