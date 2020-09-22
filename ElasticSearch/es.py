import elasticsearch
import json


def connect():
    _es = elasticsearch.Elasticsearch([{'host': 'localhost', 'port': 9200}])
    if _es.ping():
        print('Connected')
    else:
        print('Not connected')
    return _es


def insert(es, doc_index, item_id, item):
    es.index(index=doc_index, id=item_id, body=item)


def get(es, doc_index, item_id):
    try:
        return es.get(index=doc_index, id=item_id)
    except elasticsearch.exceptions.NotFoundError:
        return 'Document is not found'


def delete(es, doc_index, item_id):
    try:
        es.delete(index=doc_index, id=item_id)
    except elasticsearch.exceptions.NotFoundError:
        return 'Document has already been deleted'


def match_all(es, doc_index):
    return es.search(index=doc_index, body={
        'query': {
            'match_all': {}
        }
    })


def match(es, matching, doc_index, field, value):
    matches = ['match', 'match_phrase']
    if matching in matches:
        return es.search(index=doc_index, body={
            'query': {
                matching: {
                    field: value}
            }
        })
    else:
        return 'Wrong query'


def analyze(i, doc_index, tokenizer, text):
    return i.analyze(index=doc_index, body={
        'tokenizer': tokenizer,
        'text': text
    })


def display(results):
    print('Total entries - %d. First 10 results are shown:' % results['hits']['total']['value'])
    j = 1
    for hit in results['hits']['hits']:
        print(j, "- %(name)s %(origin)s: %(meaning)s" % hit["_source"])
        j += 1


# get data from a file
with open("foreign_names.json", "r") as f:
    names = json.loads(f.read())
name_id = 1
names_index = 'names'
elastic = connect()
# indexing (storing) a doc
for name in names:
    insert(elastic, names_index, name_id, name)
    name_id += 1
# get item
res = get(elastic, names_index, 284)
print(res)
# delete item
delete(elastic, names_index, 284)
res = get(elastic, names_index, 284)
print(res)
# get all the results
res = match_all(elastic, names_index)
print('%d results in total.' % res['hits']['total']['value'])
# get items that contain either lotus or flower
res = match(elastic, 'match', names_index, 'meaning', 'lotus flower')
display(res)
# get items that contain exactly 'lotus flower'
res = match(elastic, 'match_phrase', names_index, 'meaning', 'lotus flower')
display(res)
# and
res = elastic.search(index=names_index, body={
    'query': {
        'bool': {
            'must': [
                {
                    'match': {
                        'origin': 'English'
                    }
                },
                {
                    'match_phrase': {
                        'meaning': 'flower name'
                    }
                }
            ]
        }
    }
})
display(res)
# not
res = elastic.search(index=names_index, body={
    'query': {
        'bool': {
            'must_not': [
                {
                    'match': {
                        'origin': 'English'
                    }
                },
                {
                    'match_phrase': {
                        'meaning': 'flower name'
                    }
                }
            ]
        }
    }
})
display(res)
# or
res = elastic.search(index=names_index, body={
    'query': {
        'bool': {
            'should': [
                {
                    'match': {
                        'origin': 'English'
                    }
                },
                {
                    'match_phrase': {
                        'meaning': 'flower name'
                    }
                }
            ]
        }
    }
})
display(res)
# filter and sort
res = elastic.search(index=names_index, body={
    'query': {
        'bool': {
            'must': {
                'match': {
                    'origin': 'German'
                }
            },
            'filter': {
                'range': {
                    'PeoplesCount': {
                        'gt': 400000
                    }
                }
            }
        }
    }
})
display(res)
res = elastic.search(index=names_index, body={
    'query': {
        'range': {
            'PeoplesCount': {
                'gte': 200000
            }
        }
    },
    'sort': [
        {
            'PeoplesCount': 'desc'
        }
    ]
})
display(res)
ind = elasticsearch.client.IndicesClient(elastic)
print(analyze(ind, names_index, 'uax_url_email', 'My email: test@gmail.com'))
print(analyze(ind, names_index, 'ngram', 'My email: test@gmail.com'))
print(analyze(ind, names_index, 'path_hierarchy', 'Users/Default/Desktop'))
# usage of mappings
mapping = {
    'mappings': {
        'properties': {
            'desc': {
                'type': 'text',
                'analyzer': 'russian'
            }
        }
    }
}
response = ind.create(index='weather', body=mapping)
print(response)
el1 = {
    'desc': 'погода в Казани'
}
el2 = {
    'desc': 'дождь в городе Казань'
}
insert(elastic, 'weather', 1, el1)
insert(elastic, 'weather', 2, el2)
res = elastic.search(index='weather', body={
    'query': {
        'match': {
            'desc': 'найти значение погоды в городе Казани'
        }
    }
})
print(res)
