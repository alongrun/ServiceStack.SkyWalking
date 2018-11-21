using System;
using System.Data;
using System.Data.Common;

namespace ServiceStack.SkyWalking
{
    public class SpywalkingDbTransaction : DbTransaction
    {
        private readonly TimeSpan timerTimeSpan;

        public SpywalkingDbTransaction(DbTransaction transaction, SpywalkingDbConnection connection)
        {
            InnerTransaction = transaction;
            InnerConnection = connection;
            TransactionId = Guid.NewGuid();
        }

        public SpywalkingDbConnection InnerConnection { get; set; }

        public DbTransaction InnerTransaction { get; set; }

        public Guid TransactionId { get; set; }

        public override IsolationLevel IsolationLevel
        {
            get { return InnerTransaction.IsolationLevel; }
        }

        protected override DbConnection DbConnection
        {
            get { return InnerConnection; }
        }

        public override void Commit()
        {
            InnerTransaction.Commit();
        }

        public override void Rollback()
        {
            InnerTransaction.Rollback();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                InnerTransaction.Dispose();
            }

            base.Dispose(disposing);
        }

    }
}
