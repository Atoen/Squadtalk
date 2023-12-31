﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Squadtalk.Server.Models;

#nullable disable

namespace Squadtalk.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20231111151323_ChannelUser2")]
    partial class ChannelUser2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ChannelUser", b =>
                {
                    b.Property<Guid>("ChannelsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ParticipantsId")
                        .HasColumnType("uuid");

                    b.HasKey("ChannelsId", "ParticipantsId");

                    b.HasIndex("ParticipantsId");

                    b.ToTable("ChannelUser");
                });

            modelBuilder.Entity("Squadtalk.Server.Models.Channel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("Squadtalk.Server.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ChannelId")
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("Squadtalk.Server.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("Salt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ChannelUser", b =>
                {
                    b.HasOne("Squadtalk.Server.Models.Channel", null)
                        .WithMany()
                        .HasForeignKey("ChannelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Squadtalk.Server.Models.User", null)
                        .WithMany()
                        .HasForeignKey("ParticipantsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Squadtalk.Server.Models.Message", b =>
                {
                    b.HasOne("Squadtalk.Server.Models.User", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Squadtalk.Server.Models.Embed", "Embed", b1 =>
                        {
                            b1.Property<int>("MessageId")
                                .HasColumnType("integer");

                            b1.Property<string>("DataJson")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<int>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("MessageId");

                            b1.ToTable("Messages");

                            b1.WithOwner()
                                .HasForeignKey("MessageId");
                        });

                    b.Navigation("Author");

                    b.Navigation("Embed");
                });

            modelBuilder.Entity("Squadtalk.Server.Models.User", b =>
                {
                    b.OwnsMany("Squadtalk.Server.Models.RefreshToken", "RefreshTokens", b1 =>
                        {
                            b1.Property<Guid>("UserId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<DateTime>("Created")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime>("Expires")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("Revoked")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<string>("Token")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("UserId", "Id");

                            b1.ToTable("RefreshToken");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });

                    b.Navigation("RefreshTokens");
                });
#pragma warning restore 612, 618
        }
    }
}
