from flask import Blueprint, request, jsonify
from flask_jwt_extended import create_access_token, jwt_required, get_jwt_identity, get_jwt
from app import db
from app.models.user import User
from werkzeug.security import check_password_hash

auth_bp = Blueprint('auth', __name__)

@auth_bp.route('/login', methods=['POST'])
def login():
    data = request.get_json()
    email = data.get('email', '').strip().lower()
    password = data.get('password', '')
    user = User.query.filter_by(email=email).first()
    if not user or not user.check_password(password):
        return jsonify({'error': 'Invalid email or password'}), 401
    token = create_access_token(
        identity=str(user.id),
        additional_claims={'email': user.email, 'role': user.role}
    )
    return jsonify({
        'access_token': token,
        'user': {
            'id': user.id,
            'full_name': user.full_name,
            'email': user.email,
            'role': user.role,
            'membership_id': user.membership_id
        }
    })

@auth_bp.route('/me', methods=['GET'])
@jwt_required()
def me():
    user_id = int(get_jwt_identity())
    user = User.query.get_or_404(user_id)
    claims = get_jwt()
    return jsonify({
        'id': user.id,
        'full_name': user.full_name,
        'email': user.email,
        'role': claims.get('role', user.role),
        'membership_id': user.membership_id
    })

@auth_bp.route('/register', methods=['POST'])
def register():
    data = request.get_json()
    required = ['email', 'password', 'full_name']
    if not all(k in data for k in required):
        return jsonify({'error': 'Missing required fields'}), 400
    email = data['email'].strip().lower()
    if User.query.filter_by(email=email).first():
        return jsonify({'error': 'Email already registered'}), 409
    import uuid
    user = User(
        email=email,
        full_name=data['full_name'],
        membership_id='LIB-' + str(uuid.uuid4())[:8].upper(),
        role='Member'
    )
    user.set_password(data['password'])
    db.session.add(user)
    db.session.commit()
    return jsonify({'message': 'Registered successfully', 'membership_id': user.membership_id}), 201
