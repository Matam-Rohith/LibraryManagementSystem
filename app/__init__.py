from flask import Flask
from flask_sqlalchemy import SQLAlchemy
from flask_jwt_extended import JWTManager
from flask_migrate import Migrate
from pymongo import MongoClient
from flasgger import Swagger
import sentry_sdk
from config import Config

db = SQLAlchemy()
jwt = JWTManager()
migrate = Migrate()
mongo_client = None
mongo_db = None

def create_app(config_class=Config):
    app = Flask(__name__)
    app.config.from_object(config_class)

    db.init_app(app)
    jwt.init_app(app)
    migrate.init_app(app, db)

    global mongo_client, mongo_db
    mongo_client = MongoClient(app.config['MONGO_URI'])
    mongo_db = mongo_client[app.config['MONGO_DB_NAME']]

    activity_logs = mongo_db['activity_logs']
    activity_logs.create_index('timestamp', expireAfterSeconds=60 * 60 * 24 * 90)

    Swagger(app, template={
        'info': {
            'title': 'Library Management System API',
            'version': '1.0',
            'description': 'REST API for managing books, members, borrowing, reservations, and fines.'
        },
        'securityDefinitions': {
            'Bearer': {
                'type': 'apiKey',
                'name': 'Authorization',
                'in': 'header',
                'description': 'Enter: Bearer <jwt_token>'
            }
        },
        'security': [{'Bearer': []}]
    })

    from app.routes.auth import auth_bp
    from app.routes.books import books_bp
    from app.routes.borrow import borrow_bp
    from app.routes.reservations import reservations_bp
    from app.routes.fines import fines_bp
    from app.routes.activity_logs import logs_bp
    from app.routes.open_library import open_library_bp
    from app.routes.health import health_bp

    app.register_blueprint(auth_bp, url_prefix='/api/auth')
    app.register_blueprint(books_bp, url_prefix='/api/books')
    app.register_blueprint(borrow_bp, url_prefix='/api/borrow')
    app.register_blueprint(reservations_bp, url_prefix='/api/reservations')
    app.register_blueprint(fines_bp, url_prefix='/api/fines')
    app.register_blueprint(logs_bp, url_prefix='/api/activitylogs')
    app.register_blueprint(open_library_bp, url_prefix='/api/openlibrary')
    app.register_blueprint(health_bp)

    return app
