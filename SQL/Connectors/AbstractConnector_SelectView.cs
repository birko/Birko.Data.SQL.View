﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Birko.Data.SQL.Connectors
{
    public abstract partial class AbstractConnector
    {
        public void SelectView(Type type, Action<object> readAction, LambdaExpression expr, IDictionary<string, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            SelectView(type, readAction, (expr != null) ? DataBase.ParseConditionExpression(expr) : null, orderFields, limit, offset);
        }

        public void SelectView<T, P>(Type type, Action<object> readAction, LambdaExpression expr, IDictionary<Expression<Func<T, P>>, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            SelectView(type, readAction, (expr != null) ? DataBase.ParseConditionExpression(expr) : null, orderFields?.ToDictionary(x => DataBase.GetViewField(x.Key).GetSelectName(true), x => x.Value), limit, offset);
        }

        public void SelectView(Type type, Action<object> readAction, IEnumerable<Conditions.Condition> conditions = null, IDictionary<string, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            Select(DataBase.LoadView(type), (fields, reader) => {
                if (readAction != null)
                {
                    var data = Activator.CreateInstance(type, new object[0]);
                    DataBase.ReadView(reader, data);
                    readAction(data);
                }
            }, conditions, orderFields, limit, offset);
        }

        public void Select(Tables.View view, Action<IDictionary<int, string>, DbDataReader> readAction, LambdaExpression expr, IDictionary<string, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            Select(view, readAction, (expr != null) ? DataBase.ParseConditionExpression(expr) : null, orderFields, limit, offset);
        }

        public void Select<T, P>(Tables.View view, Action<IDictionary<int, string>, DbDataReader> readAction, LambdaExpression expr, IDictionary<Expression<Func<T, P>>, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            Select(view, readAction, (expr != null) ? DataBase.ParseConditionExpression(expr) : null, orderFields?.ToDictionary(x => DataBase.GetViewField(x.Key).GetSelectName(true), x => x.Value), limit, offset);
        }

        public void Select(Tables.View view, Action<IDictionary<int, string>, DbDataReader> readAction = null, IEnumerable<Conditions.Condition> conditions = null, IDictionary<string, bool> orderFields = null, int? limit = null, int? offset = null)
        {
            if (view != null)
            {
                DoCommand((command) => {
                    command = CreateSelectCommand(command, view, conditions, orderFields, limit, offset);
                }, (command) =>
                {
                    var reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {

                        bool isNext = reader.Read();
                        while (isNext)
                        {
                            readAction?.Invoke(view.GetSelectFields(), reader);
                            isNext = reader.Read();
                        }
                    }
                });
            }
        }
    }
}
