while getopts "v:" flag;
do
    case "${flag}" in
        v) version=${OPTARG};;
    esac
done

if [ -z "${version}" ]
then
  echo "Version must be defined e.g. $0 -v 1.0"
  exit
fi

dotnet publish -c Release
docker login
docker build -t belaorosz/teams-presence:$version .
docker push belaorosz/teams-presence:$version
docker manifest rm belaorosz/teams-presence:latest
docker manifest create belaorosz/teams-presence:latest --amend belaorosz/teams-presence:$version
docker manifest push belaorosz/teams-presence:latest
docker manifest rm belaorosz/teams-presence:$version
docker manifest create belaorosz/teams-presence:$version --amend belaorosz/teams-presence:$version
docker manifest push belaorosz/teams-presence:$version