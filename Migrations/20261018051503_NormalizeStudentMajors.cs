using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BTL_QuanLySinhVien.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeStudentMajors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    MajorId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.MajorId);
                });

            migrationBuilder.InsertData(
                table: "Majors",
                columns: new[] { "MajorId", "Name" },
                values: new object[] { "CNT", "CNT" });

            migrationBuilder.AddColumn<string>(
                name: "MajorId",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [dbo].[Students]
                    WHERE DATALENGTH(LTRIM(RTRIM([Major]))) <> DATALENGTH(N'CNT')
                       OR LTRIM(RTRIM([Major])) COLLATE Latin1_General_100_BIN2
                            <> N'CNT' COLLATE Latin1_General_100_BIN2
                )
                BEGIN
                    THROW 51000, 'Student Major values contain entries not mapped by NormalizeStudentMajors.', 1;
                END;

                UPDATE [dbo].[Students]
                SET [MajorId] = N'CNT'
                WHERE LTRIM(RTRIM([Major])) COLLATE Latin1_General_100_BIN2
                    = N'CNT' COLLATE Latin1_General_100_BIN2;

                IF EXISTS (SELECT 1 FROM [dbo].[Students] WHERE [MajorId] IS NULL)
                BEGIN
                    THROW 51001, 'Some Students rows could not be mapped to a major.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "MajorId",
                table: "Students",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_MajorId",
                table: "Students",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_Name",
                table: "Majors",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Majors_MajorId",
                table: "Students",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropColumn(
                name: "Major",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Majors_MajorId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_MajorId",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "Major",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [student]
                SET [Major] = [major].[Name]
                FROM [dbo].[Students] AS [student]
                INNER JOIN [dbo].[Majors] AS [major]
                    ON [student].[MajorId] = [major].[MajorId];

                IF EXISTS (SELECT 1 FROM [dbo].[Students] WHERE [Major] IS NULL)
                BEGIN
                    THROW 51002, 'Some Students rows could not be restored to the legacy Major column.', 1;
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Major",
                table: "Students",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "MajorId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Majors");
        }
    }
}
