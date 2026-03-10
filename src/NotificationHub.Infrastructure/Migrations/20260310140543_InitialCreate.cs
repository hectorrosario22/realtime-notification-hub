using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QueuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EmailTo = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    EmailFrom = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    EmailReplyTo = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    IsHtml = table.Column<bool>(type: "boolean", nullable: true),
                    AttachmentUrls = table.Column<List<string>>(type: "text[]", nullable: true),
                    SmsPhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    SmsFromNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    WhatsAppPhoneNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    TemplateId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WhatsAppHeaderParams = table.Column<List<string>>(type: "text[]", nullable: true),
                    WhatsAppBodyParams = table.Column<List<string>>(type: "text[]", nullable: true),
                    WhatsAppFooterParams = table.Column<List<string>>(type: "text[]", nullable: true),
                    EmailBcc = table.Column<string>(type: "jsonb", nullable: true),
                    EmailCc = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                table: "Notifications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
