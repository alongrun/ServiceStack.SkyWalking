using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using SkyWalking.Context;
using SkyWalking.Context.Tag;
using SkyWalking.Context.Trace;
using SkyWalking.NetworkProtocol.Trace;


namespace ServiceStack.SkyWalking
{
    public class LocalTrace:IDisposable
    
    {
        private const string TRACE_ORM = "TRACE_ORM";
        protected string ResolveOperationName(IDbCommand sqlCommand)
        {
            var commandType = sqlCommand.CommandText?.Split(' ');
            return $"StackService.SkyWaling Db:{commandType?.FirstOrDefault()}";
        }
        public  LocalTrace(DbCommand sqlCommand)

        {
          /*(  if (ContextManager.ContextProperties.ContainsKey(TRACE_ORM))
            {
                return;
            }*/
            var peer = sqlCommand.Connection.DataSource;
            var span = ContextManager.CreateExitSpan(ResolveOperationName(sqlCommand), peer);
            span.SetLayer(SpanLayer.DB);
            span.SetComponent(ComponentsDefine.SqlClient);

            Tags.DbType.Set(span, sqlCommand.GetType().FullName);
            Tags.DbInstance.Set(span, sqlCommand.Connection.Database);
            Tags.DbStatement.Set(span, sqlCommand.CommandText);
            if (sqlCommand.Parameters.Count > 0)
            {
                List<string> ls=new List<string>();

                for (int i=0;i<sqlCommand.Parameters.Count;i++)
                {
                    ls.Add($"{sqlCommand.Parameters[i].ParameterName}:{sqlCommand.Parameters[i].Value.ToString()}");
                    
                    
                }

                string vars = string.Join(";",ls.ToArray());
                Tags.DbBindVariables.Set(span,vars);
            }
            
        }
        
        public void OnErrorExecute(Exception ex)
        {
            /*if (ContextManager.ContextProperties.ContainsKey(TRACE_ORM))
            {
                return;
            }*/
            var span = ContextManager.ActiveSpan;
            span?.ErrorOccurred();
            span?.Log(ex);
            ContextManager.StopSpan(span);
        }


        public void Dispose()
        {
            /*  if (ContextManager.ContextProperties.ContainsKey(TRACE_ORM))
              {
                  return;
              }*/
            var span = ContextManager.ActiveSpan;
            ContextManager.StopSpan();
        }
    }
}
