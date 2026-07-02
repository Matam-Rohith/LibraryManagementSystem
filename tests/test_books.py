import pytest
from app import db
from app.models.book import Book

def test_get_books_requires_auth(client):
    resp = client.get('/api/books')
    assert resp.status_code == 401

def test_get_books_authenticated(client, admin_token):
    resp = client.get('/api/books', headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 200
    assert isinstance(resp.get_json(), list)

def test_create_book_as_admin(client, admin_token):
    payload = {
        'title': 'Clean Code',
        'author': 'Robert Martin',
        'isbn': '978-0132350884',
        'genre': 'Programming',
        'total_copies': 3
    }
    resp = client.post('/api/books', json=payload, headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 201
    data = resp.get_json()
    assert data['title'] == 'Clean Code'
    assert data['available_copies'] == 3

def test_create_book_as_member_forbidden(client, member_token):
    payload = {
        'title': 'Forbidden Book',
        'author': 'Someone',
        'isbn': '000-000-000',
        'total_copies': 1
    }
    resp = client.post('/api/books', json=payload, headers={'Authorization': f'Bearer {member_token}'})
    assert resp.status_code == 403

def test_create_book_missing_fields(client, admin_token):
    resp = client.post('/api/books', json={'title': 'Incomplete'},
                       headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 400

def test_get_book_by_id(client, admin_token):
    resp = client.get('/api/books/1', headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code in [200, 404]

def test_delete_book_as_admin(client, admin_token, app):
    with app.app_context():
        book = Book(title='To Delete', author='Author', isbn='DEL-001', total_copies=1, available_copies=1)
        db.session.add(book)
        db.session.commit()
        book_id = book.id
    resp = client.delete(f'/api/books/{book_id}', headers={'Authorization': f'Bearer {admin_token}'})
    assert resp.status_code == 200
