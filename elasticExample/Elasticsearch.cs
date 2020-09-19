using Elasticsearch.Net;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elasticExample
{
    public class Elasticsearch
    {
        private Uri _node;
        private StaticConnectionPool _pool;
        private ConnectionSettings _settings;
        private ElasticClient _client;
        private string defaultIndex;

        public Elasticsearch( string address, string index = null )
        {
            defaultIndex = !string.IsNullOrEmpty( index ) ? index : "default";

            _node = new Uri( address );
            _settings = new ConnectionSettings( _node ).DefaultIndex( defaultIndex );
            _client = new ElasticClient( _settings );
        }

        public Elasticsearch( IEnumerable<Uri> addresses )
        {
            _pool = new StaticConnectionPool( addresses );
            _settings = new ConnectionSettings( _pool );
            _client = new ElasticClient( _settings );
        }

        /// <summary>
        /// Индексирование документа
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public async Task<Result> IndexBook( BookModel book )
        {
            IndexResponse response = await _client.IndexDocumentAsync( book );
            return response.Result;
        }

        /// <summary>
        /// Получение записи по ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BookModel GetById(int id)
        {
            GetResponse<BookModel> response = _client.Get<BookModel>( id );
            return response.Source;
        }

        /// <summary>
        /// Метод поиска
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<BookModel>> SearchBook( string query )
        {
            var searchResponse = await _client.SearchAsync<BookModel>( s => s
            .From( 0 )
            .Size( 10 )
            .Query( q => q
                    .Match( m => m
                        .Field( f => f.Author )
                        .Query( query )
                        )
                    )
            );

            return searchResponse.Documents;
        }

        /// <summary>
        /// Метод Удаления записи
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Result> DeleteBook(int id )
        {
            Id Id = new Id( id );
            DeleteResponse response = await _client.DeleteAsync( new DocumentPath<BookModel>( Id ) );
            return response.Result;
        }
    }
}
