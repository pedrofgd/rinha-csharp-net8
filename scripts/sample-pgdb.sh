docker run \
    -it \
    --rm \
    --name rinha-backend-pgdb \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=123 \
    -e POSTGRES_DB=rinha \
    -p 5432:5432 \
    -v ./init.sql:/docker-entrypoint-initdb.d/init.sql:ro \
    postgres
