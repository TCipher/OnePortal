using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnePortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    ActorEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActorRoleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    PortalId = table.Column<int>(type: "int", nullable: true),
                    PortalCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationality",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationality", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Portals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubDepartment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDepartment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubDepartment_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortalRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortalRoles_Portals_PortalId",
                        column: x => x.PortalId,
                        principalTable: "Portals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsersDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReportingManagerId = table.Column<int>(type: "int", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    SubDepartmentId = table.Column<int>(type: "int", nullable: true),
                    EngageDate = table.Column<DateTime>(type: "date", nullable: true),
                    Level = table.Column<short>(type: "smallint", nullable: false),
                    WorkLocation = table.Column<short>(type: "smallint", nullable: false),
                    NationalityId = table.Column<string>(type: "nvarchar(3)", nullable: true),
                    JobType = table.Column<short>(type: "smallint", nullable: true),
                    SkillType = table.Column<short>(type: "smallint", nullable: true),
                    Gender = table.Column<short>(type: "smallint", nullable: false),
                    OriginState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LGA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Community = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ALAllowed = table.Column<int>(type: "int", nullable: true),
                    ALTaken = table.Column<int>(type: "int", nullable: true),
                    CLAllowed = table.Column<int>(type: "int", nullable: true),
                    CLTaken = table.Column<int>(type: "int", nullable: true),
                    IsReleased = table.Column<bool>(type: "bit", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsUserUpdated = table.Column<bool>(type: "bit", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Createddate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifiedby = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modifieddate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRSA = table.Column<bool>(type: "bit", nullable: false),
                    IsEmail = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserSignature = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IsOneportalUser = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordLastChangedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false),
                    PreferredMfaMethod = table.Column<short>(type: "smallint", nullable: false),
                    MfaEnrolledUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailOtpHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailOtpExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailOtpLastSentUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailOtpFailedCount = table.Column<short>(type: "smallint", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersDetails_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersDetails_Designation_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsersDetails_GlobalRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "GlobalRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsersDetails_Nationality_NationalityId",
                        column: x => x.NationalityId,
                        principalTable: "Nationality",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsersDetails_SubDepartment_SubDepartmentId",
                        column: x => x.SubDepartmentId,
                        principalTable: "SubDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_UsersDetails_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_UsersDetails_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserPortalAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PortalId = table.Column<int>(type: "int", nullable: false),
                    PortalRoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPortalAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPortalAccess_PortalRoles_PortalRoleId",
                        column: x => x.PortalRoleId,
                        principalTable: "PortalRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPortalAccess_Portals_PortalId",
                        column: x => x.PortalId,
                        principalTable: "Portals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPortalAccess_UsersDetails_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WebAuthnCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CredentialId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SignCount = table.Column<long>(type: "bigint", nullable: false),
                    Aaguid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TransportsCsv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttestationFmt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebAuthnCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebAuthnCredentials_UsersDetails_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "GlobalRoles",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "SUPERADMIN", "Super Admin" },
                    { 2, "ADMIN", "Admin" },
                    { 3, "USER", "User" }
                });

            migrationBuilder.InsertData(
                table: "Portals",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "SIMS", "Sustainability" },
                    { 2, "EMP", "Exit Management" },
                    { 3, "BILLING", "Billing" },
                    { 4, "REGULATORY", "Regulatory" },
                    { 5, "E-SERVICE", "E-Service" },
                    { 6, "PROCURE", "Procurement" },
                    { 7, "FIN", "Finance" },
                    { 8, "DIGI-DOCS", "Digi docs" },
                    { 9, "ASSET", "Asset Mgt" },
                    { 10, "TP", "Truck Park" },
                    { 11, "IMG", "Immigration" },
                    { 12, "LT", "Lease Tracking" },
                    { 13, "VM", "Visitor Mgt" },
                    { 14, "EF", "Equipment Efficiency" },
                    { 15, "SES", "Security Excellence System" },
                    { 16, "PT", "Project Tracking" },
                    { 17, "ES", "Engagement Survey" }
                });

            migrationBuilder.InsertData(
                table: "PortalRoles",
                columns: new[] { "Id", "Code", "Name", "PortalId" },
                values: new object[,]
                {
                    { 1011, "ADMIN", "Admin", 1 },
                    { 1012, "MANAGER", "Manager", 1 },
                    { 1013, "EMPLOYEE", "Employee", 1 },
                    { 1021, "ADMIN", "Admin", 2 },
                    { 1022, "MANAGER", "Manager", 2 },
                    { 1023, "EMPLOYEE", "Employee", 2 },
                    { 1031, "ADMIN", "Admin", 3 },
                    { 1032, "MANAGER", "Manager", 3 },
                    { 1033, "EMPLOYEE", "Employee", 3 },
                    { 1041, "ADMIN", "Admin", 4 },
                    { 1042, "MANAGER", "Manager", 4 },
                    { 1043, "EMPLOYEE", "Employee", 4 },
                    { 1051, "ADMIN", "Admin", 5 },
                    { 1052, "MANAGER", "Manager", 5 },
                    { 1053, "EMPLOYEE", "Employee", 5 },
                    { 1061, "ADMIN", "Admin", 6 },
                    { 1062, "MANAGER", "Manager", 6 },
                    { 1063, "EMPLOYEE", "Employee", 6 },
                    { 1071, "ADMIN", "Admin", 7 },
                    { 1072, "MANAGER", "Manager", 7 },
                    { 1073, "EMPLOYEE", "Employee", 7 },
                    { 1081, "ADMIN", "Admin", 8 },
                    { 1082, "MANAGER", "Manager", 8 },
                    { 1083, "EMPLOYEE", "Employee", 8 },
                    { 1091, "ADMIN", "Admin", 9 },
                    { 1092, "MANAGER", "Manager", 9 },
                    { 1093, "EMPLOYEE", "Employee", 9 },
                    { 1101, "ADMIN", "Admin", 10 },
                    { 1102, "MANAGER", "Manager", 10 },
                    { 1103, "EMPLOYEE", "Employee", 10 },
                    { 1111, "ADMIN", "Admin", 11 },
                    { 1112, "MANAGER", "Manager", 11 },
                    { 1113, "EMPLOYEE", "Employee", 11 },
                    { 1121, "ADMIN", "Admin", 12 },
                    { 1122, "MANAGER", "Manager", 12 },
                    { 1123, "EMPLOYEE", "Employee", 12 },
                    { 1131, "ADMIN", "Admin", 13 },
                    { 1132, "MANAGER", "Manager", 13 },
                    { 1133, "EMPLOYEE", "Employee", 13 },
                    { 1141, "ADMIN", "Admin", 14 },
                    { 1142, "MANAGER", "Manager", 14 },
                    { 1143, "EMPLOYEE", "Employee", 14 },
                    { 1151, "ADMIN", "Admin", 15 },
                    { 1152, "MANAGER", "Manager", 15 },
                    { 1153, "EMPLOYEE", "Employee", 15 },
                    { 1161, "ADMIN", "Admin", 16 },
                    { 1162, "MANAGER", "Manager", 16 },
                    { 1163, "EMPLOYEE", "Employee", 16 },
                    { 1171, "ADMIN", "Admin", 17 },
                    { 1172, "MANAGER", "Manager", 17 },
                    { 1173, "EMPLOYEE", "Employee", 17 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_PortalId",
                table: "AuditLogs",
                column: "PortalId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TimestampUtc",
                table: "AuditLogs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Department_Code",
                table: "Department",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Designation_Code",
                table: "Designation",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalRoles_Code",
                table: "GlobalRoles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_TokenHash",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "TokenHash" });

            migrationBuilder.CreateIndex(
                name: "IX_PortalRoles_PortalId_Code",
                table: "PortalRoles",
                columns: new[] { "PortalId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portals_Code",
                table: "Portals",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDepartment_DepartmentId",
                table: "SubDepartment",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalAccess_PortalId",
                table: "UserPortalAccess",
                column: "PortalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalAccess_PortalRoleId",
                table: "UserPortalAccess",
                column: "PortalRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPortalAccess_UserId_PortalId",
                table: "UserPortalAccess",
                columns: new[] { "UserId", "PortalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_DepartmentId",
                table: "UsersDetails",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_DesignationId",
                table: "UsersDetails",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_EmailAddress",
                table: "UsersDetails",
                column: "EmailAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_NationalityId",
                table: "UsersDetails",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_RoleId",
                table: "UsersDetails",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersDetails_SubDepartmentId",
                table: "UsersDetails",
                column: "SubDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WebAuthnCredentials_CredentialId",
                table: "WebAuthnCredentials",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebAuthnCredentials_UserId",
                table: "WebAuthnCredentials",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "UserPortalAccess");

            migrationBuilder.DropTable(
                name: "WebAuthnCredentials");

            migrationBuilder.DropTable(
                name: "PortalRoles");

            migrationBuilder.DropTable(
                name: "UsersDetails");

            migrationBuilder.DropTable(
                name: "Portals");

            migrationBuilder.DropTable(
                name: "Designation");

            migrationBuilder.DropTable(
                name: "GlobalRoles");

            migrationBuilder.DropTable(
                name: "Nationality");

            migrationBuilder.DropTable(
                name: "SubDepartment");

            migrationBuilder.DropTable(
                name: "Department");
        }
    }
}
