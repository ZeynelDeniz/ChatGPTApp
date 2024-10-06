using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ChatGPTApp.Models;

public partial class ChatAppDbContext : DbContext
{
    public ChatAppDbContext()
    {
    }

    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-2BJQGDD\\SQLEXPRESS; Initial Catalog=ChatAppDb; Integrated Security=True; TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC07E7D47B13");

            entity.Property(e => e.ShowInChatbox).HasDefaultValue(true);

            entity.HasOne(d => d.User).WithMany(p => p.ChatMessages).HasConstraintName("FK_ChatMessage_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC076709FB18");

            entity.Property(e => e.InitialMessageSent)
            .HasDefaultValue(false)
            .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
