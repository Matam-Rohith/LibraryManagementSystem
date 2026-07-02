from unittest.mock import patch, MagicMock

def test_search_requires_auth(client):
    resp = client.get('/api/openlibrary/search?query=python')
    assert resp.status_code == 401

def test_search_missing_query(client, admin_token):
    resp = client.get('/api/openlibrary/search', headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 400

def test_search_returns_results(client, admin_token):
    mock_response = MagicMock()
    mock_response.json.return_value = {
        'docs': [
            {'title': 'Python Crash Course', 'author_name': ['Eric Matthes'], 'first_publish_year': 2015}
        ]
    }
    with patch('app.routes.open_library.requests.get', return_value=mock_response):
        resp = client.get('/api/openlibrary/search?query=python',
                          headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 200
    data = resp.get_json()
    assert len(data) == 1
    assert data[0]['title'] == 'Python Crash Course'

def test_isbn_not_found(client, admin_token):
    mock_response = MagicMock()
    mock_response.json.return_value = {}
    with patch('app.routes.open_library.requests.get', return_value=mock_response):
        resp = client.get('/api/openlibrary/isbn/000-000',
                          headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 404
