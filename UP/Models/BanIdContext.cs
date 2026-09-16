using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BanID.Models;

public partial class BanIdContext : DbContext
{
    public BanIdContext()
    {
    }

    public BanIdContext(DbContextOptions<BanIdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BankAccount> BankAccounts { get; set; }

    public virtual DbSet<Deposit> Deposits { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDatum> UserData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-5IPG7EG;Database=BanID;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.IdAccount);

            entity.HasIndex(e => e.NumberAccount, "UQ_BankAccounts_Number_Account").IsUnique();

            entity.Property(e => e.IdAccount).HasColumnName("ID_Account");
            entity.Property(e => e.BalanceAccount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Balance_Account");
            entity.Property(e => e.CurrencyAccount)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasDefaultValue("RUB", "DF_BankAccounts_Currency_Account")
                .HasColumnName("Currency_Account");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.NumberAccount)
                .HasMaxLength(34)
                .HasColumnName("Number_Account");
            entity.Property(e => e.StatusAccount)
                .HasMaxLength(30)
                .HasDefaultValue("Active", "DF_BankAccounts_Status_Account")
                .HasColumnName("Status_Account");
            entity.Property(e => e.TypeAccount)
                .HasMaxLength(30)
                .HasColumnName("Type_Account");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.BankAccounts)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BankAccounts_Users");
        });

        modelBuilder.Entity<Deposit>(entity =>
        {
            entity.HasKey(e => e.IdDeposit);

            entity.Property(e => e.IdDeposit).HasColumnName("ID_Deposit");
            entity.Property(e => e.AmountDeposit)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Amount_Deposit");
            entity.Property(e => e.EndDeposit).HasColumnName("End_Deposit");
            entity.Property(e => e.IdAccount).HasColumnName("ID_Account");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.NameDeposit)
                .HasMaxLength(80)
                .HasColumnName("Name_Deposit");
            entity.Property(e => e.RateDeposit)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("Rate_Deposit");
            entity.Property(e => e.StatusDeposit)
                .HasMaxLength(30)
                .HasDefaultValue("Active", "DF_Deposits_Status_Deposit")
                .HasColumnName("Status_Deposit");

            entity.HasOne(d => d.IdAccountNavigation).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.IdAccount)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_BankAccounts");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Deposits)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Deposits_Users");
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.HasKey(e => e.IdRequest);

            entity.Property(e => e.IdRequest).HasColumnName("ID_Request");
            entity.Property(e => e.CommentRequest)
                .HasMaxLength(1000)
                .HasColumnName("Comment_Request");
            entity.Property(e => e.CreatedRequest)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_Requests_Created_Request")
                .HasColumnName("Created_Request");
            entity.Property(e => e.IdAccount).HasColumnName("ID_Account");
            entity.Property(e => e.IdAdmin).HasColumnName("ID_Admin");
            entity.Property(e => e.IdDeposit).HasColumnName("ID_Deposit");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.StatusRequest)
                .HasMaxLength(30)
                .HasDefaultValue("New", "DF_Requests_Status_Request")
                .HasColumnName("Status_Request");
            entity.Property(e => e.TextRequest)
                .HasMaxLength(1000)
                .HasColumnName("Text_Request");
            entity.Property(e => e.TypeRequest)
                .HasMaxLength(40)
                .HasColumnName("Type_Request");

            entity.HasOne(d => d.IdAccountNavigation).WithMany(p => p.Requests)
                .HasForeignKey(d => d.IdAccount)
                .HasConstraintName("FK_Requests_BankAccounts");

            entity.HasOne(d => d.IdAdminNavigation).WithMany(p => p.RequestIdAdminNavigations)
                .HasForeignKey(d => d.IdAdmin)
                .HasConstraintName("FK_Requests_Admin");

            entity.HasOne(d => d.IdDepositNavigation).WithMany(p => p.Requests)
                .HasForeignKey(d => d.IdDeposit)
                .HasConstraintName("FK_Requests_Deposits");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.RequestIdUserNavigations)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requests_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.HasIndex(e => e.HashLoginUser, "UQ_Users_Hash_Login_User").IsUnique();

            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.HashLoginUser)
                .HasMaxLength(64)
                .HasColumnName("Hash_Login_User");
            entity.Property(e => e.HashPassUser)
                .HasMaxLength(64)
                .HasColumnName("Hash_Pass_User");
            entity.Property(e => e.IsActiveUser)
                .HasDefaultValue(true, "DF_Users_Is_Active_User")
                .HasColumnName("Is_Active_User");
            entity.Property(e => e.RoleUser)
                .HasMaxLength(20)
                .HasColumnName("Role_User");
            entity.Property(e => e.SaltPassUser)
                .HasMaxLength(7)
                .HasColumnName("Salt_Pass_User");
        });

        modelBuilder.Entity<UserDatum>(entity =>
        {
            entity.HasKey(e => e.IdData);

            entity.HasIndex(e => e.IdUser, "UQ_UserData_ID_User").IsUnique();

            entity.Property(e => e.IdData).HasColumnName("ID_Data");
            entity.Property(e => e.EmailData)
                .HasMaxLength(300)
                .HasColumnName("Email_Data");
            entity.Property(e => e.FirstNameData)
                .HasMaxLength(300)
                .HasColumnName("First_Name_Data");
            entity.Property(e => e.IdUser).HasColumnName("ID_User");
            entity.Property(e => e.LastNameData)
                .HasMaxLength(300)
                .HasColumnName("Last_Name_Data");
            entity.Property(e => e.MiddleNameData)
                .HasMaxLength(300)
                .HasColumnName("Middle_Name_Data");
            entity.Property(e => e.PhoneData)
                .HasMaxLength(300)
                .HasColumnName("Phone_Data");
            entity.Property(e => e.SaltData)
                .HasMaxLength(7)
                .HasColumnName("Salt_Data");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.UserDatum)
                .HasForeignKey<UserDatum>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserData_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
