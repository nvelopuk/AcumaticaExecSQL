using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;


namespace ExecSQL
{
    public class ExecSQLGraph : PXGraph<ExecSQLGraph>
    {
        private const string _defaultHtml = "<style>.execSql { font-family: Arial, \"Helvetica Neue\", Helvetica, sans-serif !important; padding: 3px; } .execSQL td, .execSql th { padding: 3px; } .execSql thead { background-color: #ccc; } </style>";
        public PXCancel<ExecSQLQueryRow> Cancel;

        public SelectFrom<ExecSQLQueryRow>
            .View
                QueryView;

        public string HtmlTable { get; set; }

        public ExecSQL()
        {

        }

        public IEnumerable queryView()
        {
            List<ExecSQLQueryRow> result = new List<ExecSQLQueryRow>();

            if (QueryView.Cache.Current != null)
            {
                var current = (ExecSQLQueryRow)QueryView.Cache.Current;
                result.Add(new ExecSQLQueryRow { Selected = true, QueryResult = current.QueryResult, QueryToRun = current.QueryToRun });
            }

            return result;

        }

        [PXCacheName("ExecSQLQueryRow")]
        public class ExecSQLQueryRow : PXBqlTable, IBqlTable
        {
            public abstract class selected : IBqlField
            {

            }
            protected bool? _Selected = false;

            /// <summary>
            /// Indicates whether the record is selected for mass processing.
            /// </summary>
            [PXBool]
            [PXDefault(false)]
            [PXUIField(DisplayName = "Selected")]
            public bool? Selected
            {
                get
                {
                    return _Selected;
                }
                set
                {
                    _Selected = value;
                }
            }

            [PXString(8000)]
            [PXDefault("")]
            [PXUIField(DisplayName = "Query")]
            public virtual string QueryToRun { get; set; }
            public abstract class queryToRun : PX.Data.BQL.BqlString.Field<queryToRun> { }

            [PXString(8000)]
            [PXUIField(DisplayName = "Result")]
            public virtual string QueryResult { get; set; }
            public abstract class queryResult : PX.Data.BQL.BqlString.Field<queryResult> { }
        }

        public PXAction<ExecSQLQueryRow> runQuery;

        [PXUIField(DisplayName = "Run Query", Visible = true, Visibility = PXUIVisibility.Visible)]
        [PXButton]
        public virtual IEnumerable RunQuery(PXAdapter adapter)
        {
            if (!PXLongOperation.Exists(UID))
            {
                Actions["runQueryHidden"].PressButton();
            }

            return adapter.Get();
        }

        public PXAction<ExecSQLQueryRow> runQueryHidden;

        [PXUIField(DisplayName = "Run Query Hidden", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXButton]
        public virtual IEnumerable RunQueryHidden(PXAdapter adapter)
        {

            string queryToRun = QueryView.Cache.Current != null ? ((ExecSQLQueryRow)QueryView.Cache.Current).QueryToRun : string.Empty;

            if (string.IsNullOrEmpty(queryToRun))
                return adapter.Get();

            PXLongOperation.StartOperation(UID, () =>
            {
                List<PXSPParameter> pars = new List<PXSPParameter>();

                try
                {
                    PXSPParameter sql = new PXSPInParameter("@sql", PXDbType.NVarChar, queryToRun);
                    PXSPParameter result = new PXSPOutParameter("@result", PXDbType.NVarChar, 8000, "");

                    pars.Add(sql);
                    pars.Add(result);
                    var results = PXDatabase.Execute("spExecSQL", pars.ToArray());

                    if (results != null && results.Any())
                    {
                        var current = (ExecSQLQueryRow)QueryView.Cache.Current;

                        if (current != null)
                        {
                            current.QueryResult = GenerateHtmlTable(results?[0]?.ToString());
                            QueryView.Cache.Current = current;
                            QueryView.Cache.Update(current);
                        }
                    }

                }
                catch (Exception ex)
                {
                    var current = (ExecSQLQueryRow)QueryView.Cache.Current;

                        if (current != null)
                        {
                            current.QueryResult = _defaultHtml + "<table class=\"execSQL\"><tr><td>Error: " + ex.Message + "</td></tr></table>";
                            QueryView.Cache.Current = current;
                            QueryView.Cache.Update(current);
                        }
                }

            });

            PXLongOperation.WaitCompletion(UID);

            return adapter.Get();
        }

        protected string GenerateHtmlTable(string queryResult)
        {
            if (string.IsNullOrEmpty(queryResult))
                return _defaultHtml + "<table class=\"execSQL\"><tr><td>Please run a query</td></tr></table>";

            XDocument doc = XDocument.Parse(queryResult);
            List<string> colNames = new List<string>();

            if (!doc.Descendants("Row").Any())
                return _defaultHtml + "<table class=\"execSQL\"><tr><td>Result was empty</td></tr></table>";

            foreach (var col in doc.Descendants("Row").First().Elements())
            {
                colNames.Add(col.Name.LocalName);
            }

            string html = _defaultHtml + "<table class=\"execSQL\"><thead>";

            foreach(var colName in colNames)
            {
                html += $"<th>{colName}</th>";
            }

            html += "</thead><tbody>";

            foreach (var row in doc.Descendants("Row"))
            {
                html += "<tr>";

                foreach (var col in row.Elements())
                {
                    html += $"<td>{col.Value}</td>";

                }

                html += "</tr>";
            }

            html += "</tbody></table>";

            return html;

        }
    }
}

