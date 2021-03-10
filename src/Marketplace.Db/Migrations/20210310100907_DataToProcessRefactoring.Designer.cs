﻿// <auto-generated />
using System;
using Marketplace.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Marketplace.Db.Migrations
{
    [DbContext(typeof(MarketplaceDbContext))]
    [Migration("20210310100907_DataToProcessRefactoring")]
    partial class DataToProcessRefactoring
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("Marketplace.Db.Models.KusamaProcessedBlock", b =>
                {
                    b.Property<decimal>("BlockNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("ProcessDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("BlockNumber");

                    b.ToTable("KusamaProcessedBlocks");
                });

            modelBuilder.Entity("Marketplace.Db.Models.KusamaTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccountPublicKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Amount")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("BlockId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LockTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AccountPublicKey");

                    b.HasIndex("BlockId");

                    b.HasIndex("Status", "LockTime")
                        .HasFilter("\"Status\" = 0");

                    b.ToTable("KusamaTransactions");
                });

            modelBuilder.Entity("Marketplace.Db.Models.NftIncomeTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("CollectionId")
                        .HasColumnType("bigint");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTime?>("LockTime")
                        .IsConcurrencyToken()
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("OwnerPublicKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<long>("TokenId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("UniqueProcessedBlockId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UniqueProcessedBlockId");

                    b.HasIndex("Status", "LockTime")
                        .HasFilter("\"Status\" = 0");

                    b.ToTable("NftIncomeTransactions");
                });

            modelBuilder.Entity("Marketplace.Db.Models.Offer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("CollectionId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Metadata")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("OfferStatus")
                        .HasColumnType("integer");

                    b.Property<string>("Price")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Seller")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("SellerPublicKeyBytes")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<decimal>("TokenId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("CreationDate");

                    b.ToTable("Offers");
                });

            modelBuilder.Entity("Marketplace.Db.Models.Trade", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Buyer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("OfferId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("TradeDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("OfferId");

                    b.ToTable("Trades");
                });

            modelBuilder.Entity("Marketplace.Db.Models.UniqueProcessedBlock", b =>
                {
                    b.Property<decimal>("BlockNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("ProcessDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("BlockNumber");

                    b.ToTable("UniqueProcessedBlocks");
                });

            modelBuilder.Entity("Marketplace.Db.Models.KusamaTransaction", b =>
                {
                    b.HasOne("Marketplace.Db.Models.KusamaProcessedBlock", "Block")
                        .WithMany()
                        .HasForeignKey("BlockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Block");
                });

            modelBuilder.Entity("Marketplace.Db.Models.NftIncomeTransaction", b =>
                {
                    b.HasOne("Marketplace.Db.Models.UniqueProcessedBlock", "UniqueProcessedBlock")
                        .WithMany()
                        .HasForeignKey("UniqueProcessedBlockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UniqueProcessedBlock");
                });

            modelBuilder.Entity("Marketplace.Db.Models.Trade", b =>
                {
                    b.HasOne("Marketplace.Db.Models.Offer", "Offer")
                        .WithMany("Trades")
                        .HasForeignKey("OfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Offer");
                });

            modelBuilder.Entity("Marketplace.Db.Models.Offer", b =>
                {
                    b.Navigation("Trades");
                });
#pragma warning restore 612, 618
        }
    }
}
