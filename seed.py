from app import create_app, db
from app.models.user import User, Role
from werkzeug.security import generate_password_hash

app = create_app()

with app.app_context():
    db.create_all()

    for role_name in ['Admin', 'Member']:
        if not Role.query.filter_by(name=role_name).first():
            db.session.add(Role(name=role_name))
    db.session.commit()

    if not User.query.filter_by(email='admin@library.com').first():
        admin_role = Role.query.filter_by(name='Admin').first()
        admin = User(
            full_name='Library Admin',
            email='admin@library.com',
            membership_id='LIB-ADMIN-001',
            password_hash=generate_password_hash('Admin@123456'),
            role_id=admin_role.id
        )
        db.session.add(admin)
        db.session.commit()
        print('Admin seeded: admin@library.com / Admin@123456')
    else:
        print('Admin already exists.')
