from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from app import db
from app.models.reservation import Reservation
from app.models.book import Book

reservations_bp = Blueprint('reservations', __name__)

@reservations_bp.route('', methods=['GET'])
@jwt_required()
def get_reservations():
    claims = get_jwt()
    user_id = get_jwt_identity()
    if claims['role'] == 'Admin':
        records = Reservation.query.all()
    else:
        records = Reservation.query.filter_by(user_id=int(user_id)).all()
    return jsonify([r.to_dict() for r in records])

@reservations_bp.route('', methods=['POST'])
@jwt_required()
def create_reservation():
    data = request.get_json()
    book = Book.query.get_or_404(data.get('book_id'))
    user_id = int(get_jwt_identity())
    reservation = Reservation(user_id=user_id, book_id=book.id)
    db.session.add(reservation)
    db.session.commit()
    return jsonify(reservation.to_dict()), 201

@reservations_bp.route('/<int:res_id>/cancel', methods=['PUT'])
@jwt_required()
def cancel_reservation(res_id):
    res = Reservation.query.get_or_404(res_id)
    res.status = 'Cancelled'
    db.session.commit()
    return jsonify(res.to_dict())
