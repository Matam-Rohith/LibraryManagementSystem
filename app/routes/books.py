from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt
from app import db
from app.models.book import Book
from app.utils.decorators import admin_required
from app.services.activity_log_service import log_activity
from flask_jwt_extended import get_jwt_identity

books_bp = Blueprint('books', __name__)

@books_bp.route('', methods=['GET'])
@jwt_required()
def get_books():
    books = Book.query.all()
    return jsonify([b.to_dict() for b in books])

@books_bp.route('/<int:book_id>', methods=['GET'])
@jwt_required()
def get_book(book_id):
    book = Book.query.get_or_404(book_id)
    return jsonify(book.to_dict())

@books_bp.route('', methods=['POST'])
@jwt_required()
@admin_required
def create_book():
    data = request.get_json()
    required = ['title', 'author', 'isbn', 'total_copies']
    if not all(k in data for k in required):
        return jsonify({'error': 'Missing required fields'}), 400

    if Book.query.filter_by(isbn=data['isbn']).first():
        return jsonify({'error': 'ISBN already exists'}), 409

    book = Book(
        title=data['title'],
        author=data['author'],
        isbn=data['isbn'],
        genre=data.get('genre'),
        total_copies=data['total_copies'],
        available_copies=data['total_copies']
    )
    db.session.add(book)
    db.session.commit()
    claims = get_jwt()
    log_activity('BOOK_CREATED', get_jwt_identity(), claims['email'], claims['role'],
                 entity_type='Book', entity_id=str(book.id), details=book.title)
    return jsonify(book.to_dict()), 201

@books_bp.route('/<int:book_id>', methods=['PUT'])
@jwt_required()
@admin_required
def update_book(book_id):
    book = Book.query.get_or_404(book_id)
    data = request.get_json()
    for field in ['title', 'author', 'genre', 'total_copies']:
        if field in data:
            setattr(book, field, data[field])
    db.session.commit()
    return jsonify(book.to_dict())

@books_bp.route('/<int:book_id>', methods=['DELETE'])
@jwt_required()
@admin_required
def delete_book(book_id):
    book = Book.query.get_or_404(book_id)
    db.session.delete(book)
    db.session.commit()
    return jsonify({'message': 'Book deleted'})
