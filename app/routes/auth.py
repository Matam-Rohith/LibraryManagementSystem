from flask import Blueprint, request, jsonify
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity, get_jwt
from werkzeug.security import generate_password_hash, check_password_hash
from app import db
from app.models.user import User, Role
from app.services.activity_log_service import log_activity

auth_bp = Blueprint('auth', __name__)

@auth_bp.route('/register', methods=['POST'])
def register():
    data = request.get_json()
    required = ['full_name', 'email', 'password', 'membership_id']
    if not all(k in data for k in required):
        return jsonify({'error': 'Missing required fields'}), 400

    if User.query.filter_by(email=data['email']).first():
        return jsonify({'error': 'Email already registered'}), 409

    member_role = Role.query.filter_by(name='Member').first()
    user = User(
        full_name=data['full_name'],
        email=data['email'],
        membership_id=data['membership_id'],
        password_hash=generate_password_hash(data['password']),
        role_id=member_role.id
    )
    db.session.add(user)
    db.session.commit()
    return jsonify(user.to_dict()), 201

@auth_bp.route('/login', methods=['POST'])
def login():
    data = request.get_json()
    user = User.query.filter_by(email=data.get('email')).first()
    if not user or not check_password_hash(user.password_hash, data.get('password', '')):
        return jsonify({'error': 'Invalid credentials'}), 401

    token = create_access_token(
        identity=str(user.id),
        additional_claims={'role': user.role.name, 'email': user.email}
    )
    log_activity('LOGIN', str(user.id), user.email, user.role.name,
                 ip_address=request.remote_addr)
    return jsonify({'access_token': token, 'user': user.to_dict()})

@auth_bp.route('/me', methods=['GET'])
@jwt_required()
def me():
    user_id = get_jwt_identity()
    user = User.query.get_or_404(int(user_id))
    return jsonify(user.to_dict())
