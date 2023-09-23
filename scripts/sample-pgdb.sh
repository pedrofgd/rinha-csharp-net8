docker run \
    -it \
    --rm \
    --name rinha-backend-pgdb \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=123 \
    -e POSTGRES_DB=rinha \
    -p 5432:5432 \
    -v ./postgresql.conf:/etc/postgresql.conf:ro \
    -v ./init.sql:/docker-entrypoint-initdb.d/init.sql:ro \
    --memory=2000m \
    --cpus=0.5 \
    postgres postgres -c config_file=/etc/postgresql.conf
