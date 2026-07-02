from flask import Blueprint, jsonify
from app import db, mongo_db

health_bp = Blueprint('health', __name__)

@health_bp.route('/health', methods=['GET'])
def health():
    status = {'status': 'ok', 'postgres': 'ok', 'mongodb': 'ok'}
    try:
        db.session.execute(db.text('SELECT 1'))
    except Exception as e:
        status['postgres'] = str(e)
        status['status'] = 'degraded'
    try:
        mongo_db.command('ping')
    except Exception as e:
        status['mongodb'] = str(e)
        status['status'] = 'degraded'
    code = 200 if status['status'] == 'ok' else 503
    return jsonify(status), code
