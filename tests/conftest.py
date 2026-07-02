import pytest
from app import create_app, db
from app.models.user import User, Role
from app.models.book import Book
from config import TestingConfig
from werkzeug.security import generate_password_hash
from flask_jwt_extended import create_access_token

@pytest.fixture(scope='session')
def app():
    app = create_app(TestingConfig)
    with app.app_context():
        db.create_all()
        for role_name in ['Admin', 'Member']:
            if not Role.query.filter_by(name=role_name).first():
                db.session.add(Role(name=role_name))
        db.session.commit()
        yield app
        db.drop_all()

@pytest.fixture(scope='session')
def client(app):
    return app.test_client()

@pytest.fixture(scope='session')
def admin_token(app):
    with app.app_context():
        role = Role.query.filter_by(name='Admin').first()
        user = User(
            full_name='Test Admin',
            email='testadmin@library.com',
            membership_id='TEST-ADMIN-001',
            password_hash=generate_password_hash('Admin@123456'),
            role_id=role.id
        )
        db.session.add(user)
        db.session.commit()
        token = create_access_token(
            identity=str(user.id),
            additional_claims={'role': 'Admin', 'email': user.email}
        )
        return token

@pytest.fixture(scope='session')
def member_token(app):
    with app.app_context():
        role = Role.query.filter_by(name='Member').first()
        user = User(
            full_name='Test Member',
            email='testmember@library.com',
            membership_id='TEST-MEM-001',
            password_hash=generate_password_hash('Member@123456'),
            role_id=role.id
        )
        db.session.add(user)
        db.session.commit()
        token = create_access_token(
            identity=str(user.id),
            additional_claims={'role': 'Member', 'email': user.email}
        )
        return token
