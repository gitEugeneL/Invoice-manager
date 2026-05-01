up:
	docker compose up -d --build

down:
	docker compose down
	
down-and-clean:
	docker compose down -v