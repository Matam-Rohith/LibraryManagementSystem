import requests
from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required

open_library_bp = Blueprint('open_library', __name__)

BASE_URL = 'https://openlibrary.org'

@open_library_bp.route('/search', methods=['GET'])
@jwt_required()
def search_books():
    query = request.args.get('query', '').strip()
    limit = int(request.args.get('limit', 10))
    if not query:
        return jsonify({'error': 'query parameter is required'}), 400
    try:
        resp = requests.get(
            f'{BASE_URL}/search.json',
            params={'q': query, 'limit': limit, 'fields': 'title,author_name,first_publish_year,isbn,subject,cover_i'},
            timeout=10
        )
        data = resp.json()
        results = []
        for doc in data.get('docs', []):
            cover_id = doc.get('cover_i')
            results.append({
                'title': doc.get('title', ''),
                'authors': doc.get('author_name', []),
                'first_publish_year': doc.get('first_publish_year'),
                'isbn': (doc.get('isbn') or [None])[0],
                'subject': (doc.get('subject') or [None])[0],
                'cover_url': f'https://covers.openlibrary.org/b/id/{cover_id}-M.jpg' if cover_id else None
            })
        return jsonify(results)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@open_library_bp.route('/isbn/<isbn>', methods=['GET'])
@jwt_required()
def get_by_isbn(isbn):
    try:
        resp = requests.get(
            f'{BASE_URL}/api/books',
            params={'bibkeys': f'ISBN:{isbn}', 'format': 'json', 'jscmd': 'data'},
            timeout=10
        )
        data = resp.json()
        key = f'ISBN:{isbn}'
        if key not in data:
            return jsonify({'error': f'No book found for ISBN {isbn}'}), 404
        book = data[key]
        return jsonify({
            'title': book.get('title', ''),
            'authors': [a.get('name') for a in book.get('authors', [])],
            'isbn': isbn,
            'cover_url': book.get('cover', {}).get('medium')
        })
    except Exception as e:
        return jsonify({'error': str(e)}), 500
