using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Servis_Centar_Za_Gitare.Migrations
{
    /// <inheritdoc />
    public partial class MakeRepairTechnicianOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nalozi_Zaposlenici_TehnicarId",
                table: "Nalozi");

            migrationBuilder.AlterColumn<long>(
                name: "TehnicarId",
                table: "Nalozi",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Nalozi_Zaposlenici_TehnicarId",
                table: "Nalozi",
                column: "TehnicarId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nalozi_Zaposlenici_TehnicarId",
                table: "Nalozi");

            migrationBuilder.AlterColumn<long>(
                name: "TehnicarId",
                table: "Nalozi",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Nalozi_Zaposlenici_TehnicarId",
                table: "Nalozi",
                column: "TehnicarId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
