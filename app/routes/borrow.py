from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from datetime import datetime, timedelta
from app import db
from app.models.borrow import BorrowRecord
from app.models.book import Book
from app.models.fine import Fine
from app.services.activity_log_service import log_activity

borrow_bp = Blueprint('borrow', __name__)

FINE_PER_DAY = 5.0

@borrow_bp.route('', methods=['GET'])
@jwt_required()
def get_borrows():
    claims = get_jwt()
    user_id = get_jwt_identity()
    if claims['role'] == 'Admin':
        records = BorrowRecord.query.all()
    else:
        records = BorrowRecord.query.filter_by(user_id=int(user_id)).all()
    return jsonify([r.to_dict() for r in records])

@borrow_bp.route('', methods=['POST'])
@jwt_required()
def borrow_book():
    data = request.get_json()
    book_id = data.get('book_id')
    book = Book.query.get_or_404(book_id)
    if book.available_copies < 1:
        return jsonify({'error': 'No copies available'}), 400

    user_id = int(get_jwt_identity())
    due = datetime.utcnow() + timedelta(days=data.get('days', 14))
    record = BorrowRecord(user_id=user_id, book_id=book_id, due_date=due)
    book.available_copies -= 1
    db.session.add(record)
    db.session.commit()
    claims = get_jwt()
    log_activity('BOOK_BORROWED', str(user_id), claims['email'], claims['role'],
                 entity_type='Book', entity_id=str(book_id), details=book.title)
    return jsonify(record.to_dict()), 201

@borrow_bp.route('/<int:record_id>/return', methods=['PUT'])
@jwt_required()
def return_book(record_id):
    record = BorrowRecord.query.get_or_404(record_id)
    if record.is_returned:
        return jsonify({'error': 'Already returned'}), 400

    now = datetime.utcnow()
    record.returned_at = now
    record.is_returned = True
    record.book.available_copies += 1

    fine_amount = 0.0
    if now > record.due_date:
        days_late = (now - record.due_date).days
        fine_amount = days_late * FINE_PER_DAY
        fine = Fine(borrow_record_id=record.id, amount=fine_amount)
        db.session.add(fine)

    db.session.commit()
    claims = get_jwt()
    log_activity('BOOK_RETURNED', get_jwt_identity(), claims['email'], claims['role'],
                 entity_type='BorrowRecord', entity_id=str(record_id))
    return jsonify({'record': record.to_dict(), 'fine': fine_amount})
