using DNBase.ViewModel;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;

namespace DNBase.Services
{
    public interface IElasticSearchClient
    {
        bool InsertOne<T>(T documents) where T : class;
        bool InsertMany<T>(IEnumerable<T> documents) where T : class;
        bool InsertManyWithName<T>(string nameIndex, IEnumerable<T> documents) where T : class;
        bool Update<T>(string id, dynamic updateDocument) where T : class;
        bool UpdateMany<TModel>(List<TModel> documents) where TModel : class;
        bool DeleteIndex(string indexName);
        bool DeleteDocumentById<T>(string id) where T : class;
        bool DeleteDocument<T>(string indexName, string id) where T : class;
        bool IndexExists(string indexName);
        ElasticSearchResult<T> GetAll<T>(string indexName, int size) where T : class;
        //ElasticSearchResult<T> SearchAll(ElasticSearchModelVanBanDen model);
    }

    public class ElasticSearchClient : IElasticSearchClient
    {
        protected IElasticClient _elasticClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedisClient> _logger;

        public ElasticSearchClient(IConfiguration configuration
                                  //, IElasticClient elasticClient
                                  , ILogger<RedisClient> logger)
        {
            _configuration = configuration;
            //_elasticClient = elasticClient;
            _logger = logger;

        }

        public bool InsertOne<T>(T documents) where T : class
        {
            try
            {
                var entity = Activator.CreateInstance<T>();
                var indexName = entity.GetType().Name.ToLower();
                var index = _elasticClient.Index<T>(documents, i => i
                    .Index(indexName)
                    .Id(documents.GetType().GetProperty("Id").GetValue(documents, null).ToString())
                    .Refresh(Refresh.True)
                    );
                return index.IsValid;
            }
            catch
            {
                return false;
            }
        }

        public bool InsertMany<T>(IEnumerable<T> documents) where T : class
        {
            try
            {
                var entity = Activator.CreateInstance<T>();
                var stringId = entity.GetType().Name.ToLower();
                var bulkIndexer = new BulkDescriptor();
                foreach (var document in documents)
                {
                    bulkIndexer.Index<T>(i => i
                        .Document(document)
                        .Id(document.GetType().GetProperty("Id").GetValue(document, null).ToString())
                        .Index(stringId)
                        );
                }
                var index = _elasticClient.Bulk(bulkIndexer.Refresh(Refresh.True));
                return index.IsValid;
            }
            catch
            {
                return false;
            }

        }

        public bool InsertManyWithName<T>(string nameIndex, IEnumerable<T> documents) where T : class
        {
            try
            {
                var bulkIndexer = new BulkDescriptor();
                if (nameIndex == "nhiemvucanhan")
                {
                    foreach (var document in documents)
                    {
                        bulkIndexer.Index<T>(i => i
                            .Document(document)
                            .Id((document.GetType().GetProperty("Id").GetValue(document, null)
                            + "_"
                            + document.GetType().GetProperty("PhamViId").GetValue(document, null)
                            + "_" + document.GetType().GetProperty("CanBoId").GetValue(document, null)).ToString())
                            .Index(nameIndex)
                            );
                    }
                }
                else if (nameIndex == "nhiemvudonvi")
                {
                    foreach (var document in documents)
                    {
                        bulkIndexer.Index<T>(i => i
                            .Document(document)
                            .Id((document.GetType().GetProperty("Id").GetValue(document, null)
                            + "_"
                            + document.GetType().GetProperty("PhamViId").GetValue(document, null)).ToString())
                            .Index(nameIndex)
                            );
                    }
                }
                else
                {
                    foreach (var document in documents)
                    {
                        bulkIndexer.Index<T>(i => i
                            .Document(document)
                            .Id(document.GetType().GetProperty("Id").GetValue(document, null).ToString())
                            .Index(nameIndex)
                            );
                    }
                }

                var index = _elasticClient.Bulk(bulkIndexer.Refresh(Refresh.True));
                return index.IsValid;
            }
            catch
            {
                return false;
            }

        }

        public bool Update<T>(string id, dynamic updateDocument) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance<T>();
                var indexName = instance.GetType().Name.ToLower();
                var response = _elasticClient.Update<T, dynamic>(id, u => u.Index(indexName).Doc(updateDocument).Refresh(Refresh.True));

                return response.IsValid;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateMany<TModel>(List<TModel> documents) where TModel : class
        {
            try
            {
                var instance = Activator.CreateInstance<TModel>();
                var indexName = instance.GetType().Name;
                var bulkIndexer = new BulkDescriptor();
                bulkIndexer.UpdateMany(documents, (d, doc) => d.Doc(doc).Index(indexName));

                var index = _elasticClient.Bulk(bulkIndexer.Refresh(Refresh.True));
                return index.IsValid;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteDocumentById<T>(string id) where T : class
        {
            var instance = Activator.CreateInstance<T>();
            var indexName = instance.GetType().Name.ToLower();
            var response = _elasticClient.DeleteByQuery<T>(q => q
             .Query(rq => rq
                 .Term(m => m
                 .Value(id)
                 .Field("_id")
             )).Index(indexName).Refresh(true));
            return response.IsValid;
        }

        public bool DeleteDocument<T>(string indexName, string id) where T : class
        {
            var response = _elasticClient.DeleteByQuery<T>(q => q
             .Query(rq => rq
                 .Term(m => m
                 .Value(id)
                 .Field("_id")
             )).Index(indexName).Refresh(true));
            return response.IsValid;
        }

        public bool DeleteIndex(string indexName)
        {
            try
            {
                var response = _elasticClient.Indices.Delete(indexName);

                return response.IsValid;
            }
            catch
            {
                return false;
            }
        }

        public bool IndexExists(string indexName)
        {
            return _elasticClient.Indices.Exists(indexName).Exists;
        }

        public ElasticSearchResult<T> GetAll<T>(string indexName, int size) where T : class
        {
            try
            {
                var response = _elasticClient.Search<T>(s => s
                    .Index(indexName)
                    .From(0)
                    .Size(size)
                    .Query(q => q.MatchAll()));
                if (response.IsValid)
                {
                    var searchResult = new ElasticSearchResult<T>
                    {
                        Total = response.Total,
                        Page = 0,
                        Data = (List<T>)response.Documents,
                        ElapsedMilliseconds = response.Took
                    };

                    return searchResult;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool CreateIndexSettings(List<string> filterAnalyzer = null)
        {
            try
            {
                var listFieldNameAnalyzer = new List<string>(){"trichYeu","nguoiSoanThao","soKyHieu","donViSoanThao","noiDungChiDao","nguoiXuLy","donViBanHanh","loaiVanBanDi","loaiVanBan","doKhan","soTTVB","soVanBan","chiDaoXuLy"};
                if (filterAnalyzer == null)
                {
                    filterAnalyzer = new List<string>()
                    {
                        "lowercase", "asciifolding",
                    };
                }
                CustomAnalyzer customAnlyzer = new CustomAnalyzer
                {
                    Tokenizer = "mynGram",
                    Filter = filterAnalyzer
                };

                var indexName = "test.index.name";
                IndexExists("test.index.name");
                if (!IndexExists(indexName))
                {
                    var index = _elasticClient.Indices.Create(indexName, c => c
                        .Settings(s => s
                            .Analysis(a => a
                                .Tokenizers(tz => tz
                                    .NGram("mynGram", ng => ng
                                         .MinGram(1)
                                         .MaxGram(2)
                                    )
                                )
                                 .Analyzers(an => an
                                 .UserDefined("analyzer_userdefind", customAnlyzer)
                                )
                            )
                            .Setting("index.max_result_window", 999999999)
                        )
                        .Map(mm => mm
                            .AutoMap()
                            .Properties(p => p
                            .Text(t => t
                                .Name(listFieldNameAnalyzer[0])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                            )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[1])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                            )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[2])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                            )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[3])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[4])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[5])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[6])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[7])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[8])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[9])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[10])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[11])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             .Text(t => t
                                .Name(listFieldNameAnalyzer[12])
                                .Analyzer("analyzer_userdefind")
                                .SearchAnalyzer("analyzer_userdefind")
                                )
                             )
                        )
                    );
                    return index.IsValid;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        //public ElasticSearchResult<T> SearchAll(ElasticSearchModelVanBanDen model)
        //{
        //    try
        //    {
        //        GetConnection();
        //        var instance = Activator.CreateInstance<T>();
        //        var indexName = instance.GetType().Name.ToLower();
        //        var searchRequest = new SearchDescriptor<T>().Index(indexName).From(0).Size(model.pageSize);

        //        searchRequest.Query(q => q
        //            .QueryString(qs => qs
        //                .Query("*" + model.keyword + "*")
        //                .Fields(f => f.Fields(model.fields))
        //                .DefaultOperator(Operator.Or)
        //            )
        //        );
        //        var response = elasticClient.Search<T>(searchRequest);
        //        if (response.IsValid)
        //        {
        //            var searchResult = new ElasticSearchResult<T>
        //            {
        //                Total = response.Total,
        //                Page = 0,
        //                Data = (List<T>)response.Documents,
        //                ElapsedMilliseconds = response.Took
        //            };

        //            return searchResult;
        //        }
        //        return null;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}