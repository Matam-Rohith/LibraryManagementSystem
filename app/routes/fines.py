from flask import Blueprint, request, jsonify
from flask_jwt_extended import jwt_required, get_jwt_identity, get_jwt
from app import db
from app.models.fine import Fine
from app.utils.decorators import admin_required

fines_bp = Blueprint('fines', __name__)

@fines_bp.route('', methods=['GET'])
@jwt_required()
def get_fines():
    claims = get_jwt()
    user_id = get_jwt_identity()
    if claims['role'] == 'Admin':
        fines = Fine.query.all()
    else:
        from app.models.borrow import BorrowRecord
        records = BorrowRecord.query.filter_by(user_id=int(user_id)).all()
        record_ids = [r.id for r in records]
        fines = Fine.query.filter(Fine.borrow_record_id.in_(record_ids)).all()
    return jsonify([f.to_dict() for f in fines])

@fines_bp.route('/<int:fine_id>/pay', methods=['PUT'])
@jwt_required()
def pay_fine(fine_id):
    fine = Fine.query.get_or_404(fine_id)
    fine.is_paid = True
    db.session.commit()
    return jsonify(fine.to_dict())
