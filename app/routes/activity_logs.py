from flask import Blueprint, jsonify, request
from flask_jwt_extended import jwt_required
from app.utils.decorators import admin_required
from app import mongo_db
from bson import json_util
import json

logs_bp = Blueprint('logs', __name__)

@logs_bp.route('', methods=['GET'])
@jwt_required()
@admin_required
def get_logs():
    page = int(request.args.get('page', 1))
    page_size = int(request.args.get('page_size', 50))
    skip = (page - 1) * page_size
    logs = list(
        mongo_db['activity_logs']
        .find({}, {'_id': 0})
        .sort('timestamp', -1)
        .skip(skip)
        .limit(page_size)
    )
    return jsonify(logs)

@logs_bp.route('/user/<user_id>', methods=['GET'])
@jwt_required()
@admin_required
def get_logs_by_user(user_id):
    page = int(request.args.get('page', 1))
    page_size = int(request.args.get('page_size', 50))
    skip = (page - 1) * page_size
    logs = list(
        mongo_db['activity_logs']
        .find({'user_id': user_id}, {'_id': 0})
        .sort('timestamp', -1)
        .skip(skip)
        .limit(page_size)
    )
    return jsonify(logs)
